using KModkit;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(KMBombModule), typeof(KMSelectable))]
public partial class WarsawMetroModule : MonoBehaviour
{
    enum Line
    {
        M1,
        M2
    }

    private KMBombInfo _bombInfo;
    private KMAudio _audio;
    private KMBombModule _module;
    private static int s_moduleCount;
    private int _moduleId;

    private readonly string[] _M1Stations =
    {
        "Młociny",
        "Wawrzyszew",
        "Stare Bielany",
        "Słodowiec",
        "Marymont",
        "Plac Wilsona",
        "Dworzec Gdański",
        "Ratusz Arsenał",
        "Świętokrzyska",
        "Centrum",
        "Politechnika",
        "Pole\nMokotowskie",
        "Racławicka",
        "Wierzbno",
        "Wilanowska",
        "Służew",
        "Ursynów",
        "Stokłosy",
        "Imielin",
        "Natolin",
        "Kabaty",
    };
    private readonly string[] _M2Stations =
    {
        "Bemowo",
        "Ulrychów",
        "Księcia Janusza",
        "Młynów",
        "Płocka",
        "Rondo\nDaszyńskiego",
        "Rondo ONZ",
        "Świętokrzyska",
        "Nowy Świat-\n-Uniwersytet",
        "Centrum\nNauki Kopernik",
        "Stadion\nNarodowy",
        "Dworzec\nWileński",
        "Szwedzka",
        "Targówek\nMieszkaniowy",
        "Trocka",
        "Zacisze",
        "Kondratowicza",
        "Bródno"
    };
    private readonly string[] _rightSideDoorStations =
    {
        "Centrum",
        "Słodowiec",
        "Stare Bielany",
        "Wawrzyszew",
        "Młociny"
    };
    private int _stage;
    private Line _currentLine;
    private string _currentStation;
    private Line _destinationLine;
    private string _destinationStation;
    #region References
    public GameObject Stage1, TimeTop, TimeBottom, DirectionTextTop, DirectionTextBottom, DateTimeDisplay, TrainSpriteTop, TrainSpriteBottom, StationName, StationBackground, LineSwitchSprite;
    public KMSelectable ButtonTop, ButtonBottom, SwitchLineButton;
    public GameObject Stage2, Dots;
    public KMSelectable LeaveTrainButton;
    public GameObject SolvedContainer;
    public Material BlueMaterial, RedMaterial;
    public Sprite M1Sprite, M2Sprite, OlderSprite, OldSprite, CurrentSprite, NewSprite;
    #endregion
    #region Stage 1 variables
    private int _trainTypeTop = 0;
    private int _trainTypeBottom = 0;
    private int _nextTrainTimeTop;
    private int _nextTrainTimeBottom;
    private bool _trainTopArrived = false;
    private bool _trainBottomArrived = false;
    private KMAudio.KMAudioRef _trainTopAudio;
    private KMAudio.KMAudioRef _trainBottomAudio;
    private int _type1Score = 0;
    private int _type2Score = 0;
    private int _type3Score = 0;
    private int _type4Score = 0;
    private List<int> _forbiddenTypes = new List<int>();
    #endregion
    #region Stage 2 variables
    private int _travelDirection;
    private int _nextStationTime;
    private KMAudio.KMAudioRef _ambience;
    private int _dotCount = 1;
    #endregion
    #region Coroutines
    private Coroutine _scheduleCoroutine;
    private Coroutine _travelCoroutine;
    #endregion

    // Stage 1
    private IEnumerator TrainSchedule()
    {
        while (_stage == 1)
        {
            DateTimeDisplay.GetComponent<TextMesh>().text = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            yield return new WaitForSeconds(1.0f);

            // Top train
            if (--_nextTrainTimeTop == 20 && !_trainTopArrived)
            {
                Log("Top train arrived.");
                StopAndClearAudioRef(ref _trainTopAudio);
                _trainTopAudio = _audio.PlaySoundAtTransformWithRef("TrainArrive", ButtonTop.transform);
                TimeTop.GetComponent<TextMesh>().text = "WJAZD";
                _trainTopArrived = true;
                StartCoroutine(StopRefSoundDelayed(_trainTopAudio, 11.208f));
            }
            else if (_nextTrainTimeTop == 0)
            {
                Log("Top train departed.");
                StopAndClearAudioRef(ref _trainTopAudio);
                _trainTopAudio = _audio.PlaySoundAtTransformWithRef("TrainDepart", ButtonTop.transform);
                RollTopTrain(false);
                StartCoroutine(StopRefSoundDelayed(_trainTopAudio, 6.096f));
            }
            else if (!_trainTopArrived && _nextTrainTimeTop > 0)
            {
                UpdateTrainTimeDisplay(TimeTop, _nextTrainTimeTop);
            }

            // Bottom train
            if (--_nextTrainTimeBottom == 20 && !_trainBottomArrived)
            {
                Log("Bottom train arrived.");
                StopAndClearAudioRef(ref _trainBottomAudio);
                _trainBottomAudio = _audio.PlaySoundAtTransformWithRef("TrainArrive", ButtonBottom.transform);
                TimeBottom.GetComponent<TextMesh>().text = "WJAZD";
                _trainBottomArrived = true;
                StartCoroutine(StopRefSoundDelayed(_trainBottomAudio, 11.208f));
            }
            else if (_nextTrainTimeBottom == 0)
            {
                Log("Bottom train departed.");
                StopAndClearAudioRef(ref _trainBottomAudio);
                _trainBottomAudio = _audio.PlaySoundAtTransformWithRef("TrainDepart", ButtonBottom.transform);
                RollBottomTrain(false);
                StartCoroutine(StopRefSoundDelayed(_trainBottomAudio, 6.096f));
            }
            else if (!_trainBottomArrived && _nextTrainTimeBottom > 0)
            {
                UpdateTrainTimeDisplay(TimeBottom, _nextTrainTimeBottom);
            }
        }
    }

    // Stage 2
    private IEnumerator Travel()
    {
        while (_stage == 2)
        {
            yield return new WaitForSeconds(1.0f);

            if (_dotCount > 3) _dotCount = 0;
            Dots.GetComponent<TextMesh>().text = new string('.', _dotCount++);

            // random event, 1/2000 chance
            if (Random.Range(0, 2000) == 1476)
            {
                _audio.PlaySoundAtTransform("Bagaz", Stage2.transform);
            }

            if (--_nextStationTime == 0)
            {
                Log($"This is: {_currentStation}.");
                StopAndClearAudioRef(ref _ambience);
                _audio.PlaySoundAtTransform("Arriving", Stage2.transform);
                yield return StartCoroutine(PlaySound("Stacja", Stage2.transform, 2.41f));
                yield return StartCoroutine(PlaySound(NormalizeName(_currentStation), Stage2.transform, 2.776f));
                if (_rightSideDoorStations.Contains(_currentStation))
                {
                    yield return StartCoroutine(PlaySound("DrzwiPrawej", Stage2.transform, 2.455f));
                }
                else
                {
                    yield return StartCoroutine(PlaySound("DrzwiLewej", Stage2.transform, 2.429f));
                }
                yield return new WaitForSeconds(9.03f);

                Log("Doors opened, waiting 10 seconds.");
                LeaveTrainButton.OnInteract += OnLeaveTrain;

                yield return new WaitForSeconds(10.0f);

                Log("Doors closing, proceeding to next station.");
                LeaveTrainButton.OnInteract -= OnLeaveTrain;

                if (((_currentStation == "Kabaty" || _currentStation == "Bródno") && _travelDirection == 1) || ((_currentStation == "Młociny" || _currentStation == "Bemowo") && _travelDirection == -1))
                {
                    Strike("Got kicked out of terminating train by security, strike.");
                    yield break;
                }
                ProgressStation();
                yield return StartCoroutine(PlaySound("Departing", Stage2.transform, 24.0f));
                _ambience = _audio.PlaySoundAtTransformWithRef("Ambience", Stage2.transform);

                Log($"Next station: {_currentStation}.");
                yield return StartCoroutine(PlaySound("NastepnaStacja", Stage2.transform, 2.112f));
                yield return StartCoroutine(PlaySound(NormalizeName(_currentStation), Stage2.transform, 1.776f));
                _nextStationTime = Random.Range(20, 41);
            }
        }

        StopAndClearAudioRef(ref _ambience);
    }

    private IEnumerator PlaySound(string name, Transform transform, float duration)
    {
        _audio.PlaySoundAtTransform(name, transform);
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator ProgressStage()
    {
        if (_scheduleCoroutine != null)
        {
            StopCoroutine(_scheduleCoroutine);
            _scheduleCoroutine = null;
        }
        StopAndClearAudioRef(ref _trainTopAudio);
        StopAndClearAudioRef(ref _trainBottomAudio);

        Stage1.SetActive(false);
        Stage2.SetActive(true);
        _stage = 2;
        _nextStationTime = Random.Range(20, 41);
        ProgressStation();
        yield return StartCoroutine(PlaySound("Departing", Stage2.transform, 24.0f));
        _ambience = _audio.PlaySoundAtTransformWithRef("Ambience", Stage2.transform);
        Log($"Next station: {_currentStation}");
        yield return StartCoroutine(PlaySound("NastepnaStacja", Stage2.transform, 2.112f));
        yield return StartCoroutine(PlaySound(NormalizeName(_currentStation), Stage2.transform, 1.776f));

        _travelCoroutine = StartCoroutine(Travel());
    }

    private void ProgressStation()
    {
        string[] stations = (_currentLine == Line.M1) ? _M1Stations : _M2Stations;
        int currentIndex = System.Array.FindIndex(stations, s => ReadableName(s) == _currentStation);
        int newIndex = currentIndex + _travelDirection;
        if (newIndex >= 0 && newIndex < stations.Length)
        {
            _currentStation = ReadableName(stations[newIndex]);
        }
    }

    private void RollTopTrain(bool struck)
    {
        if (struck) StopAndClearAudioRef(ref _trainTopAudio);
        _trainTopArrived = false;
        _nextTrainTimeTop = Random.Range(50, 301);
        _trainTypeTop = Random.Range(1, 5);
        if (_trainTypeTop == 1) TrainSpriteTop.GetComponent<SpriteRenderer>().sprite = OlderSprite;
        else if (_trainTypeTop == 2) TrainSpriteTop.GetComponent<SpriteRenderer>().sprite = OldSprite;
        else if (_trainTypeTop == 3) TrainSpriteTop.GetComponent<SpriteRenderer>().sprite = CurrentSprite;
        else if (_trainTypeTop == 4) TrainSpriteTop.GetComponent<SpriteRenderer>().sprite = NewSprite;

        UpdateTrainTimeDisplay(TimeTop, _nextTrainTimeTop);

        Log($"Next top train is of type {_trainTypeTop}.");
    }

    private void RollBottomTrain(bool struck)
    {
        if (struck) StopAndClearAudioRef(ref _trainBottomAudio);
        _trainBottomArrived = false;
        _nextTrainTimeBottom = Random.Range(50, 301);
        _trainTypeBottom = Random.Range(1, 5);
        if (_trainTypeBottom == 1) TrainSpriteBottom.GetComponent<SpriteRenderer>().sprite = OlderSprite;
        else if (_trainTypeBottom == 2) TrainSpriteBottom.GetComponent<SpriteRenderer>().sprite = OldSprite;
        else if (_trainTypeBottom == 3) TrainSpriteBottom.GetComponent<SpriteRenderer>().sprite = CurrentSprite;
        else if (_trainTypeBottom == 4) TrainSpriteBottom.GetComponent<SpriteRenderer>().sprite = NewSprite;

        UpdateTrainTimeDisplay(TimeBottom, _nextTrainTimeBottom);

        Log($"Next bottom train is of type {_trainTypeBottom}.");
    }

    private new void StopAllCoroutines()
    {
        if (_scheduleCoroutine != null)
        {
            StopCoroutine(_scheduleCoroutine);
            _scheduleCoroutine = null;
        }
        if (_travelCoroutine != null)
        {
            StopCoroutine(_travelCoroutine);
            _travelCoroutine = null;
        }
        StopAndClearAudioRef(ref _ambience);
        StopAndClearAudioRef(ref _trainTopAudio);
        StopAndClearAudioRef(ref _trainBottomAudio);
    }
#pragma warning disable IDE0051
    private void Awake()
    {
        _moduleId = s_moduleCount++;

        _module = GetComponent<KMBombModule>();
        _bombInfo = GetComponent<KMBombInfo>();
        _audio = GetComponent<KMAudio>();

        _module.OnActivate += Activate;
        _bombInfo.OnBombSolved += OnEnd;
        _bombInfo.OnBombExploded += OnEnd;
    }

    private void Start()
    {
        _currentLine = (Line)System.Enum.GetValues(typeof(Line)).GetValue(Random.Range(0, 2));
        if (_currentLine == Line.M1)
        {
            string rawStation = _M1Stations[Random.Range(0, _M1Stations.Length)];
            _currentStation = ReadableName(rawStation);

            StationName.GetComponent<TextMesh>().text = rawStation;
            StationBackground.GetComponent<MeshRenderer>().material = BlueMaterial;
            DirectionTextTop.GetComponent<TextMesh>().text = _M1Stations[_M1Stations.Length - 1].ToUpper();
            DirectionTextBottom.GetComponent<TextMesh>().text = _M1Stations[0].ToUpper();
            LineSwitchSprite.GetComponent<SpriteRenderer>().sprite = M2Sprite;
        }
        else if (_currentLine == Line.M2)
        {
            string rawStation = _M2Stations[Random.Range(0, _M2Stations.Length)];
            _currentStation = ReadableName(rawStation);

            StationName.GetComponent<TextMesh>().text = rawStation;
            StationBackground.GetComponent<MeshRenderer>().material = RedMaterial;
            DirectionTextTop.GetComponent<TextMesh>().text = _M2Stations[_M2Stations.Length - 1].ToUpper();
            DirectionTextBottom.GetComponent<TextMesh>().text = _M2Stations[0].ToUpper();
            LineSwitchSprite.GetComponent<SpriteRenderer>().sprite = M1Sprite;
        }
        Log($"Starting on the {_currentLine} line, station {_currentStation}.");

        #region Solution
        // Type 1 score
        if (_bombInfo.IsIndicatorOn(Indicator.CLR)) _type1Score += 3;
        char[] targets = { 'M', 'W', '2' };
        if (_bombInfo.GetSerialNumber().Any(c => targets.Contains(c))) _type1Score -= 2;
        if (!_bombInfo.IsPortPresent(Port.StereoRCA)) _type1Score += 1;

        // Type 2 score
        if (_bombInfo.IsIndicatorOff(Indicator.FRK)) _type2Score += 3;
        if (_bombInfo.IsPortPresent(Port.PS2)) _type2Score -= 2;
        if (_bombInfo.GetPortPlates().Any(plate => plate.Length == 0)) _type2Score += 1;

        // Type 3 score
        if (System.DateTime.Now.DayOfWeek == System.DayOfWeek.Tuesday) _type3Score += 3;
        if (_bombInfo.GetBatteryCount() > 3) _type3Score -= 2;
        int[] primes = { 2, 3, 5, 7 };
        if (_bombInfo.GetSerialNumberNumbers().Any(n => primes.Contains(n))) _type3Score += 1;

        // Type 4 score
        if (_bombInfo.IsPortPresent(Port.DVI)) _type4Score += 3;
        if (_bombInfo.IsPortPresent(Port.RJ45) || _currentLine == Line.M1) _type4Score -= 2;
        string snLetters = _bombInfo.GetSerialNumber().Where(char.IsLetter).Select(char.ToUpper).Join("");
        if (snLetters.Length != snLetters.Distinct().Count()) _type4Score += 1;

        // Forbidden types
        var scoreMap = new List<KeyValuePair<int, int>>
        {
            new KeyValuePair<int, int>(1, _type1Score),
            new KeyValuePair<int, int>(2, _type2Score),
            new KeyValuePair<int, int>(3, _type3Score),
            new KeyValuePair<int, int>(4, _type4Score)
        };
        List<int> lowest = new List<int>();
        foreach (var entry in scoreMap) if (entry.Value == scoreMap.Min(e => e.Value)) lowest.Add(entry.Key);
        _forbiddenTypes = lowest.Count == 4 ? new List<int>() : lowest;

        // Destination line
        int dr = DigitalRoot(_bombInfo.GetSerialNumberNumbers());
        if (dr > 0) _destinationLine = (Line)System.Math.Abs(dr % 2);
        else _destinationLine = Line.M2;

        // Destination station
        int destStationIndex = (_bombInfo.GetPortCount() + _bombInfo.GetOnIndicators().Count()) * _bombInfo.GetSerialNumberNumbers().Max();
        if (_destinationLine == Line.M1)
        {
            string rawStation = _M1Stations[destStationIndex % 21];
            _destinationStation = ReadableName(rawStation);
        }
        else
        {
            string rawStation = _M2Stations[destStationIndex % 18];
            _destinationStation = ReadableName(rawStation);
        }

        Log($"Destination: {_destinationStation}, {_destinationLine} line.");
        Log($"Train type scores (1-4): {_type1Score}, {_type2Score}, {_type3Score}, {_type4Score}.");
        string types = _forbiddenTypes.Join(", ");
        Log($"Forbidden train types: {(types.Length != 0 ? types : "None")}.");
        #endregion

        SwitchLineButton.OnInteract += OnPress;
        LeaveTrainButton.OnInteract += OnPress;
        ButtonTop.OnInteract += OnInteractTop;
        ButtonBottom.OnInteract += OnInteractBottom;
        SwitchLineButton.OnInteract += OnLineSwitch;

        Stage2.SetActive(false);
        SolvedContainer.SetActive(false);

        if (_currentStation != "Świętokrzyska")
        {
            SwitchLineButton.transform.Translate(new Vector3(0.0f, -0.035f, 0.0f));
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        _stage = 0;
    }
#pragma warning restore IDE0051
    private void Activate()
    {
        _stage = 1;
        _scheduleCoroutine = StartCoroutine(TrainSchedule());
        RollTopTrain(false);
        RollBottomTrain(false);
    }

    private void OnEnd()
    {
        StopAllCoroutines();
        _stage = 0;
    }

    private bool OnInteractTop()
    {
        Log($"Attempting to board top train of type {_trainTypeTop}.");
        if (!_trainTopArrived)
        {
            Strike("Tried to board while train not at station, strike.");
            return false;
        }
        if (_currentLine == Line.M1 && _currentStation == "Kabaty")
        {
            Strike("Boarded on terminus, strike.");
            return false;
        }
        if (_currentLine == Line.M2 && _currentStation == "Bródno")
        {
            Strike("Boarded on terminus, strike.");
            return false;
        }
        if (_forbiddenTypes.Contains(_trainTypeTop))
        {
            Strike($"Boarded forbidden train type {_trainTypeTop}, strike.");
            return false;
        }
        Log($"Successfully boarded train headed for {DirectionTextTop.GetComponent<TextMesh>().text[0] + DirectionTextTop.GetComponent<TextMesh>().text.Substring(1).ToLower()}.");
        _travelDirection = 1;
        StartCoroutine(ProgressStage());
        return false;
    }

    private bool OnInteractBottom()
    {
        Log($"Attempting to board bottom train of type {_trainTypeBottom}.");
        if (!_trainBottomArrived)
        {
            Strike("Tried to board while train not at station, strike.");
            return false;
        }
        if (_currentLine == Line.M1 && _currentStation == "Młociny")
        {
            Strike("Boarded on terminus, strike.");
            return false;
        }
        if (_currentLine == Line.M2 && _currentStation == "Bemowo")
        {
            Strike("Boarded on terminus, strike.");
            return false;
        }
        if (_forbiddenTypes.Contains(_trainTypeBottom))
        {
            Strike($"Boarded forbidden train type {_trainTypeBottom}, strike.");
            return false;
        }
        Log($"Successfully boarded train headed for {DirectionTextBottom.GetComponent<TextMesh>().text[0] + DirectionTextBottom.GetComponent<TextMesh>().text.Substring(1).ToLower()}.");
        _travelDirection = -1;
        StartCoroutine(ProgressStage());
        return false;
    }

    private bool OnLineSwitch()
    {
        Log($"Switching line to the {(Line)((int)_currentLine ^ 1)}.");
        if (_currentLine == Line.M2)
        {
            _currentLine = Line.M1;
            StationBackground.GetComponent<MeshRenderer>().material = BlueMaterial;
            DirectionTextTop.GetComponent<TextMesh>().text = "KABATY";
            DirectionTextBottom.GetComponent<TextMesh>().text = "MŁOCINY";
            LineSwitchSprite.GetComponent<SpriteRenderer>().sprite = M2Sprite;
        }
        else if (_currentLine == Line.M1)
        {
            _currentLine = Line.M2;
            StationBackground.GetComponent<MeshRenderer>().material = RedMaterial;
            DirectionTextTop.GetComponent<TextMesh>().text = "BRÓDNO";
            DirectionTextBottom.GetComponent<TextMesh>().text = "BEMOWO";
            LineSwitchSprite.GetComponent<SpriteRenderer>().sprite = M1Sprite;
        }
        RollTopTrain(false);
        RollBottomTrain(false);
        return false;
    }

    private bool OnLeaveTrain()
    {
        Log($"Left the train on {_currentStation}.");
        if (_currentStation == _destinationStation)
        {
            // Solve
            StopAllCoroutines();
            ButtonTop.OnInteract -= OnInteractTop;
            ButtonBottom.OnInteract -= OnInteractBottom;
            SwitchLineButton.OnInteract -= OnLineSwitch;
            LeaveTrainButton.OnInteract -= OnLeaveTrain;
            _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, SolvedContainer.transform);
            Stage2.SetActive(false);
            SolvedContainer.SetActive(true);
            Log("Destination achieved, module solved.");
            _module.HandlePass();
        }
        else if (_currentStation == "Świętokrzyska")
        {
            SwitchLineButton.transform.Translate(new Vector3(0.0f, 0.035f, 0.0f));
            StopAllCoroutines();
            Stage2.SetActive(false);
            Stage1.SetActive(true);
            Activate();
        }
        else
        {
            Strike("Left the train on a station that isn't the destination or a necessary transfer, strike.");
        }
        return false;
    }

    private bool OnPress()
    {
        _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Stage1.transform);
        return false;
    }

    public void Strike(string message)
    {
        Log($"{message}");
        _module.HandleStrike();

        StopAllCoroutines();

        _stage = 1;
        Stage2.SetActive(false);
        Stage1.SetActive(true);

        RollTopTrain(true);
        RollBottomTrain(true);

        // Restart the Stage 1 coroutine
        _scheduleCoroutine = StartCoroutine(TrainSchedule());
    }
}