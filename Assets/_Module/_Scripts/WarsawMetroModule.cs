// using KModkit; // You must import this namespace to use KMBombInfoExtensions, among other things. See KModKit Docs below.
using UnityEngine;

// ! Remember to remove things that you do not use, including using directives and empty methods.

// * Template Wiki: https://github.com/TheKuroEver/KTaNE-Module-Template/wiki
// * KModKit Documentation: https://github.com/Qkrisi/ktanemodkit/wiki
// ! Remember that the class and file names have to match.
[RequireComponent(typeof(KMBombModule), typeof(KMSelectable))]
public partial class WarsawMetroModule : MonoBehaviour
{
    // private KMBombInfo _bombInfo; // for accessing edgework, and certain events like OnBombExploded.
    private KMAudio _audio; // for interacting with the game's audio system.
    private KMBombModule _module;

    private static int s_moduleCount;
    private int _moduleId;
    [SerializeField] private KMSelectable _button;
    // [SerializeField] private AudioClip _buttonDown;
    // [SerializeField] private AudioClip _buttonUp;

#pragma warning disable IDE0051
    // * Called before anything else.
    private void Awake() {
        _moduleId = s_moduleCount++;

        _module = GetComponent<KMBombModule>();
        // _bombInfo = GetComponent<KMBombInfo>(); // (*)
        _audio = GetComponent<KMAudio>();

        // _module.OnActivate += Activate;
        // _bombInfo.OnBombExploded += OnBombExploded; // (**). Requires (*)
        // _bombInfo.OnBombSolved += OnBombSolved; // (***). Requires (*)

        // * Declare other references here if needed.
    }

    // * Called after Awake has been called on all components in the scene, but before anything else.
    // ! Things like querying edgework need to be done after Awake is called, eg. subscribing to OnInteract events.
    private void Start() {
        _button.OnInteract += OnInteract;
        _button.OnInteractEnded += OnInteractEnded;
    }

    // * Called once the lights turn on.
    // private void Activate() { }

    // * Update is called every frame. I don't typically use Update in the main script.
    // ! Do not perform resource-intensive tasks here as they will be called every frame and can slow the game down.
    // private void Update() { }

    // * Called when the module is removed from the game world.
    // * Examples of when this happens include when the bomb explodes, or if the player quits to the office.
    // private void OnDestroy() { }
#pragma warning restore IDE0051

    // private void OnBombExploded() { } // Requires (*) and (**)
    // private void OnBombSolved() { } // Requires (*) and (***)

    private bool OnInteract() { // has to be bool, true means give access to child selectables
        // _audio.PlaySoundAtTransform(_buttonDown.name, _button.transform);
        Strike("Tried to solve");
        return false;
    }

    private void OnInteractEnded() {
        // _audio.PlaySoundAtTransform(_buttonUp.name, _button.transform);
        Solve();
    }

    public void Log(string message) => Debug.Log($"[{_module.ModuleDisplayName} #{_moduleId}] {message}");

    public void Strike(string message) {
        Log($"✕ {message}");
        _module.HandleStrike();
        // * Add code that should execute on every strike (eg. a strike animation) here.
    }

    public void Solve() {
        Log("◯ Module solved!");
        _module.HandlePass();
        // * Add code that should execute on solve (eg. a solve animation) here.
        _button.OnInteract -= OnInteract;
        _button.OnInteractEnded -= OnInteractEnded;
    }
}
