using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WarsawMetroModule : MonoBehaviour
{
    public void Log(string message) => Debug.Log($"[{_module.ModuleDisplayName} #{_moduleId}] {message}");

    private string ReadableName(string s)
    {
        return s.Replace("-\n-", "-").Replace('\n', ' ').Trim();
    }

    private string NormalizeName(string s)
    {
        return s
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace('ę', 'e')
            .Replace('ł', 'l')
            .Replace('ń', 'n')
            .Replace('ó', 'o')
            .Replace('Ś', 'S')
            .Replace('ż', 'z');
    }

    private int DigitalRoot(int n)
    {
        if (n == 0) return 0;
        return 1 + (n - 1) % 9;
    }

    private int DigitalRoot(IEnumerable<int> seq)
    {
        int sum = 0;
        foreach (int num in seq)
        {
            sum += DigitalRoot(num);
            if (sum > 9)
            {
                sum = DigitalRoot(sum);
            }
        }
        return DigitalRoot(sum);
    }

    private IEnumerator StopRefSoundDelayed(KMAudio.KMAudioRef sound, float after)
    {
        yield return new WaitForSeconds(after);
        StopAndClearAudioRef(ref sound);
    }

    private void UpdateTrainTimeDisplay(GameObject timeObj, int timeSeconds)
    {
        TextMesh textMesh = timeObj.GetComponent<TextMesh>();
        if (timeSeconds >= 180)
        {
            textMesh.text = $"{timeSeconds / 60}min";
        }
        else
        {
            textMesh.text = string.Format("{0}:{1:D2}", timeSeconds / 60, timeSeconds % 60);
        }
    }

    private void StopAndClearAudioRef(ref KMAudio.KMAudioRef audioRef)
    {
        if (audioRef != null)
        {
            audioRef.StopSound();
            audioRef = null;
        }
    }
}
