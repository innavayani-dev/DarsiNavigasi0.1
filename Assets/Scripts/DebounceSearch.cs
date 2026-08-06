using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_InputField))]
public class DebounceSearch : MonoBehaviour
{
    [Header("Pengaturan Debounce")]
    [Tooltip("Waktu tunda dalam detik sebelum pencarian dieksekusi")]
    public float debounceTime = 0.5f;

    [Header("Event Callback")]
    [Tooltip("Event ini akan dipanggil setelah jeda waktu selesai. Sambungkan ke fungsi pencarian utama.")]
    public UnityEvent<string> onSearchExecuted;

    private TMP_InputField inputField;
    private Coroutine debounceCoroutine;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    void OnEnable()
    {
        if (inputField != null)
        {
            // Tambahkan listener saat user mengetik
            inputField.onValueChanged.AddListener(OnInputChanged);
        }
    }

    void OnDisable()
    {
        if (inputField != null)
        {
            // Hapus listener untuk mencegah memory leak
            inputField.onValueChanged.RemoveListener(OnInputChanged);
        }
    }

    private void OnInputChanged(string text)
    {
        // Jika ada timer yang sedang berjalan, batalkan
        if (debounceCoroutine != null)
        {
            StopCoroutine(debounceCoroutine);
        }
        
        // Mulai timer baru
        debounceCoroutine = StartCoroutine(DebounceRoutine(text));
    }

    private IEnumerator DebounceRoutine(string text)
    {
        // Tunggu selama waktu yang ditentukan
        yield return new WaitForSeconds(debounceTime);
        
        // Eksekusi fungsi pencarian
        ExecuteFilter(text);
    }

    private void ExecuteFilter(string keyword)
    {
        // Panggil semua fungsi yang terdaftar di UnityEvent
        onSearchExecuted?.Invoke(keyword);
    }
}
