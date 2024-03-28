using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayTrial : MonoBehaviour
{
    public static GameplayTrial instance;
    public static int level;
    public AudioSource audioSource;

    private bool isCoupled = true, isHinted = false;
    private int siblingIndex;
    private Transform[] points = new Transform[4];
    private Transform tile;

    [SerializeField]
    private AudioClip clickButtonClip, matchTileClip, switchClip;
    [SerializeField]
    private AudioClip timeWizardClip, hintClip, magicWandClip, freezeTimeClip, shuffleClip;
    [SerializeField]
    private InputField lvInput;
    [SerializeField]
    private GameObject chooseLvPanel;
    [SerializeField]
    private TextMeshProUGUI titleLv;
    [SerializeField]
    private Button pauseButton;
    [SerializeField]
    private Button btSpHint, btSpMagicWand, btSpFreeze, btSpShuffle;
    [SerializeField]
    private Transform tempAlignWithLeft, tempAlignWithRight, tempAlignWithTop, tempAlignWithBottom, tempAlignWithStart, tempAlignWithEnd;

    void _MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Awake()
    {
        _MakeInstance();
    }

    void Start()
    {
        Time.timeScale = 0;
    }

    void _ScaleBgr()
    {
        Transform temp = GameObject.Find("Background").transform;
        GameObject background = temp.GetChild(TrialBoard.levelData.Theme - 1).gameObject;
        for (int i = 0; i < temp.childCount; i++)
        {
            if (TrialBoard.levelData.Theme - 1 != i)
            {
                temp.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                temp.GetChild(i).gameObject.SetActive(true);
            }
        }

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        float height = sr.bounds.size.y;
        float width = sr.bounds.size.x;
        float scaleHeight = Camera.main.orthographicSize * 2f;
        float scaleWidth = scaleHeight * Screen.width / Screen.height;
        background.transform.localScale = new Vector3(scaleWidth / width, scaleHeight / height, 0);
    }

    #region Choose level
    public void _ChooseLevel()
    {
        audioSource.PlayOneShot(clickButtonClip);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void _PlayLvTest()
    {
        audioSource.PlayOneShot(clickButtonClip);
        var dataStr = Resources.Load("demo") as TextAsset;
        TrialBoard.levelData = JsonConvert.DeserializeObject<LevelData>(dataStr.text);
        titleLv.text = "DEMO";
        TrialBoard.instance._GoToProcess(1);
        Time.timeScale = 0;
        Timer.time = 0;
        chooseLvPanel.SetActive(false);
        _ScaleBgr();
    }

    public void _PlayLevel()
    {
        audioSource.PlayOneShot(clickButtonClip);
        level = int.Parse(lvInput.text);
        var dataStr = Resources.Load("Levels/Level_" + level) as TextAsset;
        TrialBoard.levelData = JsonConvert.DeserializeObject<LevelData>(dataStr.text);
        titleLv.text = "LEVEL " + level;
        TrialBoard.instance._GoToProcess(1);
        Time.timeScale = 0;
        Timer.time = 0;
        chooseLvPanel.SetActive(false);
        _ScaleBgr();
    }

    public void _CheckInput(InputField input)
    {
        if (input.text == "" || int.Parse(input.text) <= 0)
        {
            input.text = "1";
        }
        else if (int.Parse(input.text) > Resources.LoadAll("Levels").Length)
        {
            input.text = Resources.LoadAll("Levels").Length + "";
        }
    }

    #endregion

    #region Play trial
    public void _Pause()
    {
        audioSource.PlayOneShot(switchClip);
        pauseButton.transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void _Resume()
    {
        audioSource.PlayOneShot(switchClip);
        pauseButton.transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void _Quit()
    {
        audioSource.PlayOneShot(clickButtonClip);
        SceneManager.LoadScene("GenerateMap");
    }

    public void _ClickTile(Transform currentTile)
    {
        currentTile.DOKill();
        currentTile.GetComponent<RectTransform>().DOScale(new Vector3(.8f, .8f, 1), .1f).SetEase(Ease.InOutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                currentTile.GetComponent<RectTransform>().DOScale(Vector3.one, .1f).SetEase(Ease.InOutQuad).SetUpdate(true);
                currentTile.GetChild(0).GetComponent<Image>().DOColor(Color.green, .1f).SetEase(Ease.InOutQuad).SetUpdate(true);
            });
        Time.timeScale = 1;
        isCoupled = !isCoupled;
        if (!isCoupled)
        {
            tile = currentTile;
            siblingIndex = currentTile.GetSiblingIndex();
        }
        else
        {
            if (siblingIndex == currentTile.GetSiblingIndex())
            {
                Debug.Log("Click cung 1 tile");
                foreach (var tile in TrialBoard.buttonList)
                {
                    if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                }
                tile = currentTile;
                siblingIndex = currentTile.GetSiblingIndex();
                isCoupled = !isCoupled;
            }
            else
            {
                if (!_AreTheSameTiles(tile, currentTile))
                {
                    Debug.Log("Click khac tile khac ID");
                    foreach (var tile in TrialBoard.buttonList)
                    {
                        if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                    }
                    tile = currentTile;
                    siblingIndex = currentTile.GetSiblingIndex();
                    isCoupled = !isCoupled;
                }
                else
                {
                    if (_HasAvailableConnection(tile, currentTile))
                    {
                        StopAllCoroutines();
                        StartCoroutine(MakeConnection(points));
                        audioSource.PlayOneShot(matchTileClip);
                        StartCoroutine(DestroyTiles(points));
                    }
                    else
                    {
                        Debug.Log("Khong the ket noi");
                        foreach (var tile in TrialBoard.buttonList)
                        {
                            if (tile != currentTile) tile.GetChild(0).GetComponent<Image>().color = Color.white;
                        }
                        tile = currentTile;
                        siblingIndex = currentTile.GetSiblingIndex();
                        isCoupled = !isCoupled;
                    }
                }
            }
        }
    }

    bool _AreTheSameTiles(Transform tile1, Transform tile2)
    {
        return tile1.GetComponent<TileController>().Id == tile2.GetComponent<TileController>().Id;
    }

    void _ResetTileState()
    {
        foreach (var tile in TrialBoard.buttonList)
        {
            DOTween.Kill(tile);
            tile.localScale = Vector3.one;
        }
        isHinted = false;
    }

    #endregion

    #region Supporter & Booster
    public void _EnableSupporter(bool isEnabled)
    {
        btSpHint.interactable = isEnabled;
        btSpMagicWand.interactable = isEnabled;
        btSpFreeze.interactable = isEnabled;
        btSpShuffle.interactable = isEnabled;
    }

    public void _SupporterHint() //Hint: Khi sử dụng sẽ gợi ý 1 kết quả
    {
        audioSource.PlayOneShot(hintClip);
        _ResetTileState();
        while (!isHinted)
        {
            int index = Random.Range(1, TrialBoard.dict.Count);
            List<Transform> temp = TrialBoard.instance._SearchSameTiles(TrialBoard.dict.ElementAt(index).Value);
            if (temp.Count != 0)
            {
                for (int i = 0; i < (temp.Count - 1); i++)
                {
                    for (int j = (i + 1); j < temp.Count; j++)
                    {
                        if (_HasAvailableConnection(temp[i], temp[j]))
                        {
                            isHinted = true;
                            goto endloop;
                        }
                    }
                }
            endloop:;
            }
        }

        int n = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                n--;
            }
        }
        for (int i = 0; i < n; i++)
        {
            if (i == 0 || i == n - 1)
            {
                points[i].DOScale(new Vector3(.8f, .8f, .8f), .5f).SetEase(Ease.InOutQuad).SetUpdate(true).SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    public void _SupporterMagicWand() //Đũa thần: 2 kết quả hoàn thành
    {
        audioSource.PlayOneShot(magicWandClip);
        Time.timeScale = 1;
        _ResetTileState();
        isCoupled = true;
        foreach (var tile in TrialBoard.buttonList)
        {
            tile.GetChild(0).GetComponent<Image>().color = Color.white;
        }
        EventSystem.current.SetSelectedGameObject(null);
        int numCouple = 0;
        for (int index = 1; index < TrialBoard.dict.Count; index++)
        {
        check:
            if (numCouple == 2)
            {
                break;
            }
            List<Transform> temp = TrialBoard.instance._SearchSameTiles(TrialBoard.dict.ElementAt(index).Value);

            if (temp.Count != 0)
            {
                for (int i = 0; i < temp.Count / 2; i++)
                {
                    if (numCouple == 2)
                    {
                        goto check;
                    }

                    StopAllCoroutines();
                    StartCoroutine(DestroyTiles(temp[2 * i], temp[2 * i + 1]));
                    numCouple++;
                }
            }
        }
    }

    public void _SupporterFreezeTime() //Snow: Đóng băng thời gian 10 giây
    {
        audioSource.PlayOneShot(freezeTimeClip);
        Time.timeScale = 1;
        Timer.instance._FreezeTime(true);
        btSpFreeze.interactable = false;
        btSpFreeze.transform.DOScale(Vector3.one, 10).SetEase(Ease.InOutQuad).SetUpdate(true).OnComplete(delegate
        {
            Timer.instance._FreezeTime(false);
            btSpFreeze.interactable = true;
        });
    }

    public void _SupporterShuffle() //Đổi vị trí: Khi người chơi sử dụng, các item thay đổi vị trí cho nhau.
    {
        audioSource.PlayOneShot(shuffleClip);
        _ResetTileState();
        isCoupled = true;
        foreach (var tile in TrialBoard.buttonList)
        {
            tile.GetChild(0).GetComponent<Image>().color = Color.white;
        }
        EventSystem.current.SetSelectedGameObject(null);
        TrialBoard.instance._RearrangeTiles();
    }

    public void _BoosterTimeWizard() //Time : Tăng 10 giây khi sử dụng
    {
        audioSource.PlayOneShot(timeWizardClip);
        Timer.time += 10;
    }

    public void _BoosterEarthquake() //Remove All: Xoá 1 hình bất kỳ
    {
        audioSource.PlayOneShot(clickButtonClip);
        bool isFounded = false;
        while (!isFounded)
        {
            int index = Random.Range(1, TrialBoard.dict.Count);
            List<Transform> temp = TrialBoard.instance._SearchSameTiles(TrialBoard.dict.ElementAt(index).Value);
            if (temp.Count != 0)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    StartCoroutine(DestroyTiles(temp[i]));
                }
                isFounded = true;
            }
        }
    }

    public void _BoosterMirror() //Mirror: gấp đôi số sao đạt được
    {
        audioSource.PlayOneShot(clickButtonClip);

    }

    #endregion

    #region IEnumerator
    IEnumerator MakeConnection(params Transform[] linePositions)
    {
        LineController.instance._DrawLine(0.15f * tile.GetComponent<TileController>().Size / 100, linePositions);

        yield return new WaitForSeconds(0.4f);

        LineController.instance._EraseLine();
    }

    IEnumerator DestroyTiles(params Transform[] linePositions)
    {
        Sequence sequence = DOTween.Sequence();
        _EnableSupporter(false);
        int n = linePositions.Length;
        for (int i = 0; i < linePositions.Length; i++)
        {
            if (linePositions[i] == null)
            {
                n--;
            }
        }

        for (int i = 0; i < n; i++)
        {
            if (i == 0 || i == n - 1)
            {
                int index = i;
                linePositions[index].GetComponent<Button>().interactable = false;
                sequence.Join(linePositions[i].GetComponent<RectTransform>().DOSizeDelta(Vector2.zero, 0.4f).SetEase(Ease.InBack).SetUpdate(true)
                    .OnComplete(delegate { linePositions[index].gameObject.SetActive(false); })
                    );
                TrialBoard.instance._DeactivateTile(linePositions[i].GetComponent<TileController>());
            }
        }
        sequence.Play();
        yield return new WaitForSeconds(0.4f);
        _ResetTileState();
        TrialBoard.instance._ActivateGravity();
        yield return new WaitForSeconds(0.2f);

        _EnableSupporter(true);
        TrialBoard.instance._CheckPossibleConnection();
        TrialBoard.instance._CheckProcess();
    }

    #endregion

    #region Algorithm
    void _SetLinePositions(Transform t0, Transform t1, Transform t2, Transform t3)
    {
        points[0] = t0;
        points[1] = t1;
        points[2] = t2;
        points[3] = t3;
    }

    public bool _HasAvailableConnection(Transform tile1, Transform tile2)
    {

        #region 1 line
        // check line with x
        if (tile1.localPosition.x == tile2.localPosition.x)
        {
            if (_HasVerticalLine(tile1, tile2))
            {
                _SetLinePositions(tile1, tile2, null, null);
                return true;
            }
        }
        // check line with y
        if (tile1.localPosition.y == tile2.localPosition.y)
        {
            if (_HasHorizontalLine(tile1, tile2))
            {
                _SetLinePositions(tile1, tile2, null, null);
                return true;
            }
        }
        #endregion

        #region 2 lines
        if (_HasHorizontalLshapedPath(tile1, tile2))
        {
            return true;
        }
        if (_HasVerticalLshapedPath(tile1, tile2))
        {
            return true;
        }
        #endregion

        #region 3 lines
        if (_HasHorizontalZshapedPath(tile1, tile2))
        {
            return true;
        }
        if (_HasVerticalZshapedPath(tile1, tile2))
        {
            return true;
        }

        // check more right
        if (_HasHorizontalUshapedPath(tile1, tile2, 1))
        {
            return true;
        }
        // check more left
        if (_HasHorizontalUshapedPath(tile1, tile2, -1))
        {
            return true;
        }
        // check more down
        if (_HasVerticalUshapedPath(tile1, tile2, 1))
        {
            return true;
        }
        // check more up
        if (_HasVerticalUshapedPath(tile1, tile2, -1))
        {
            return true;
        }
        #endregion

        return false;
    }

    //button cung hang
    bool _HasHorizontalLine(Transform tile1, Transform tile2)
    {
        float y = tile1.localPosition.y;
        float min = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float max = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        if (max - min == buttonSide)
        {
            return true;
        }
        else
        {
            for (float i = min + buttonSide; i < max; i += buttonSide)
            {
                if (TrialBoard.instance._HasButtonInLocation(i, y))
                {
                    return false;
                }
            }
        }
        return true;
    }

    //button cung cot
    bool _HasVerticalLine(Transform tile1, Transform tile2)
    {
        float x = tile1.localPosition.x;
        float min = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        float max = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        if (max - min == buttonSide)
        {
            return true;
        }
        else
        {
            for (float i = min + buttonSide; i < max; i += buttonSide)
            {
                if (TrialBoard.instance._HasButtonInLocation(x, i))
                {
                    return false;
                }
            }
        }
        return true;
    }

    //2 button trong hinh chu nhat ngang: check hinh chu L
    bool _HasHorizontalLshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform leftTile, rightTile;
        float leftX = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float rightX = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        if (leftX == tile1.localPosition.x)
        {
            leftTile = tile1;
            rightTile = tile2;
        }
        else
        {
            leftTile = tile2;
            rightTile = tile1;
        }

        #endregion

        for (float i = leftX; i <= rightX; i += (rightX - leftX))
        {
            tempAlignWithLeft.localPosition = new Vector3(i, leftTile.localPosition.y, 0);
            tempAlignWithRight.localPosition = new Vector3(i, rightTile.localPosition.y, 0);
            if (i == leftX)
            {
                if (!TrialBoard.instance._HasButtonInLocation(i, rightTile.localPosition.y) &&
                    _HasVerticalLine(leftTile, tempAlignWithRight) && _HasHorizontalLine(tempAlignWithRight, rightTile))
                {
                    _SetLinePositions(leftTile, tempAlignWithRight, rightTile, null);
                    return true;
                }
            }
            else
            {
                if (!TrialBoard.instance._HasButtonInLocation(i, leftTile.localPosition.y) &&
                    _HasHorizontalLine(leftTile, tempAlignWithLeft) && _HasVerticalLine(tempAlignWithLeft, rightTile))
                {
                    _SetLinePositions(leftTile, tempAlignWithLeft, rightTile, null);
                    return true;
                }
            }
        }
        return false;
    }

    //2 button trong hinh chu nhat dung: check hinh chu L
    bool _HasVerticalLshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform topTile, bottomTile;
        float topY = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float bottomY = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        if (topY == tile1.localPosition.y)
        {
            topTile = tile1;
            bottomTile = tile2;
        }
        else
        {
            topTile = tile2;
            bottomTile = tile1;
        }

        #endregion

        for (float i = topY; i >= bottomY; i -= (topY - bottomY))
        {
            tempAlignWithTop.localPosition = new Vector3(topTile.localPosition.x, i, 0);
            tempAlignWithBottom.localPosition = new Vector3(bottomTile.localPosition.x, i, 0);
            if (i == topY)
            {
                if (!TrialBoard.instance._HasButtonInLocation(bottomTile.localPosition.x, i) &&
                    _HasHorizontalLine(topTile, tempAlignWithBottom) && _HasVerticalLine(tempAlignWithBottom, bottomTile))
                {
                    _SetLinePositions(topTile, tempAlignWithBottom, bottomTile, null);
                    return true;
                }
            }
            else
            {
                if (!TrialBoard.instance._HasButtonInLocation(topTile.localPosition.x, i) &&
                        _HasVerticalLine(topTile, tempAlignWithTop) && _HasHorizontalLine(tempAlignWithTop, bottomTile))
                {
                    _SetLinePositions(topTile, tempAlignWithStart, bottomTile, null);
                    return true;
                }
            }
        }
        return false;
    }

    //2 button trong hinh chu nhat ngang: check hinh chu Z
    bool _HasHorizontalZshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform leftTile, rightTile;
        float leftX = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float rightX = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        if (leftX == tile1.localPosition.x)
        {
            leftTile = tile1;
            rightTile = tile2;
        }
        else
        {
            leftTile = tile2;
            rightTile = tile1;
        }
        float buttonSide = tile1.GetComponent<TileController>().Size;
        bool checkPoint;

        #endregion

        for (float i = (leftX + buttonSide); i < rightX; i += buttonSide)
        {
            tempAlignWithLeft.localPosition = new Vector3(i, leftTile.localPosition.y, 0);
            tempAlignWithRight.localPosition = new Vector3(i, rightTile.localPosition.y, 0);
            checkPoint = TrialBoard.instance._HasButtonInLocation(i, leftTile.localPosition.y)
                || TrialBoard.instance._HasButtonInLocation(i, rightTile.localPosition.y);
            if (!checkPoint && _HasHorizontalLine(leftTile, tempAlignWithLeft) &&
                _HasVerticalLine(tempAlignWithLeft, tempAlignWithRight) && _HasHorizontalLine(tempAlignWithRight, rightTile))
            {
                _SetLinePositions(leftTile, tempAlignWithLeft, tempAlignWithRight, rightTile);
                return true;
            }
        }
        return false;
    }

    //2 button trong hinh chu nhat dung: check hinh chu Z
    bool _HasVerticalZshapedPath(Transform tile1, Transform tile2)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x || tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform topTile, bottomTile;
        float topY = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float bottomY = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        if (topY == tile1.localPosition.y)
        {
            topTile = tile1;
            bottomTile = tile2;
        }
        else
        {
            topTile = tile2;
            bottomTile = tile1;
        }
        float buttonSide = tile1.GetComponent<TileController>().Size;
        bool checkPoint;

        #endregion

        for (float i = (topY - buttonSide); i > bottomY; i -= buttonSide)
        {
            tempAlignWithTop.localPosition = new Vector3(topTile.localPosition.x, i, 0);
            tempAlignWithBottom.localPosition = new Vector3(bottomTile.localPosition.x, i, 0);
            checkPoint = TrialBoard.instance._HasButtonInLocation(topTile.localPosition.x, i)
                || TrialBoard.instance._HasButtonInLocation(bottomTile.localPosition.x, i);
            if (!checkPoint && _HasVerticalLine(topTile, tempAlignWithTop) &&
                _HasHorizontalLine(tempAlignWithTop, tempAlignWithBottom) && _HasVerticalLine(tempAlignWithBottom, bottomTile))
            {
                _SetLinePositions(topTile, tempAlignWithTop, tempAlignWithBottom, bottomTile);
                return true;
            }
        }
        return false;
    }

    //mo rong ra theo chieu ngang: check hinh chu U
    bool _HasHorizontalUshapedPath(Transform tile1, Transform tile2, int direct)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.y == tile2.localPosition.y)
        {
            return false;
        }

        Transform startTile, endTile;
        float lowerX = Mathf.Min(tile1.localPosition.x, tile2.localPosition.x);
        float higherX = Mathf.Max(tile1.localPosition.x, tile2.localPosition.x);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        float width = TrialBoard.column * buttonSide;
        bool checkPoint;

        #endregion

        if (direct > 0)
        {
            #region Di tu trai sang phai

            if (lowerX == tile1.localPosition.x)
            {
                startTile = tile1;
                endTile = tile2;
            }
            else
            {
                startTile = tile2;
                endTile = tile1;
            }

            for (float i = (higherX + buttonSide); i <= (width + buttonSide) / 2; i += buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(i, startTile.localPosition.y, 0);
                tempAlignWithEnd.localPosition = new Vector3(i, endTile.localPosition.y, 0);
                checkPoint = TrialBoard.instance._HasButtonInLocation(i, startTile.localPosition.y) || TrialBoard.instance._HasButtonInLocation(i, endTile.localPosition.y);
                if (!checkPoint && _HasHorizontalLine(startTile, tempAlignWithStart) &&
                    _HasVerticalLine(tempAlignWithStart, tempAlignWithEnd) && _HasHorizontalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }
        else
        {
            #region Di tu phai sang trai

            if (lowerX == tile1.localPosition.x)
            {
                startTile = tile2;
                endTile = tile1;
            }
            else
            {
                startTile = tile1;
                endTile = tile2;
            }

            for (float i = (lowerX - buttonSide); i >= -(width + buttonSide) / 2; i -= buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(i, startTile.localPosition.y, 0);
                tempAlignWithEnd.localPosition = new Vector3(i, endTile.localPosition.y, 0);
                checkPoint = TrialBoard.instance._HasButtonInLocation(i, startTile.localPosition.y) || TrialBoard.instance._HasButtonInLocation(i, endTile.localPosition.y);
                if (!checkPoint && _HasHorizontalLine(startTile, tempAlignWithStart) &&
                    _HasVerticalLine(tempAlignWithStart, tempAlignWithEnd) && _HasHorizontalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }

        return false;
    }

    //mo rong ra theo chieu doc: check hinh chu U
    bool _HasVerticalUshapedPath(Transform tile1, Transform tile2, int direct)
    {
        #region Setup toa do, kich thuoc, dieu kien

        if (tile1.localPosition.x == tile2.localPosition.x)
        {
            return false;
        }

        Transform startTile, endTile;
        float higherY = Mathf.Max(tile1.localPosition.y, tile2.localPosition.y);
        float lowerY = Mathf.Min(tile1.localPosition.y, tile2.localPosition.y);
        float buttonSide = tile1.GetComponent<TileController>().Size;
        float height = TrialBoard.row * buttonSide;
        bool checkPoint;

        #endregion

        if (direct > 0)
        {
            #region Di tu tren xuong duoi

            if (higherY == tile1.localPosition.y)
            {
                startTile = tile1;
                endTile = tile2;
            }
            else
            {
                startTile = tile2;
                endTile = tile1;
            }

            for (float i = (lowerY - buttonSide); i >= -(height + buttonSide) / 2; i -= buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(startTile.localPosition.x, i, 0);
                tempAlignWithEnd.localPosition = new Vector3(endTile.localPosition.x, i, 0);
                checkPoint = TrialBoard.instance._HasButtonInLocation(startTile.localPosition.x, i) || TrialBoard.instance._HasButtonInLocation(endTile.localPosition.x, i);
                if (!checkPoint && _HasVerticalLine(startTile, tempAlignWithStart) &&
                    _HasHorizontalLine(tempAlignWithStart, tempAlignWithEnd) && _HasVerticalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }
        else
        {
            #region Di tu duoi len tren

            if (higherY == tile1.localPosition.y)
            {
                startTile = tile2;
                endTile = tile1;
            }
            else
            {
                startTile = tile1;
                endTile = tile2;
            }

            for (float i = (higherY + buttonSide); i <= (height + buttonSide) / 2; i += buttonSide)
            {
                tempAlignWithStart.localPosition = new Vector3(startTile.localPosition.x, i, 0);
                tempAlignWithEnd.localPosition = new Vector3(endTile.localPosition.x, i, 0);
                checkPoint = TrialBoard.instance._HasButtonInLocation(startTile.localPosition.x, i) || TrialBoard.instance._HasButtonInLocation(endTile.localPosition.x, i);
                if (!checkPoint && _HasVerticalLine(startTile, tempAlignWithStart) &&
                    _HasHorizontalLine(tempAlignWithStart, tempAlignWithEnd) && _HasVerticalLine(tempAlignWithEnd, endTile))
                {
                    _SetLinePositions(startTile, tempAlignWithStart, tempAlignWithEnd, endTile);
                    return true;
                }
            }

            #endregion
        }

        return false;
    }

    #endregion

}
