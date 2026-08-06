using System.Collections.Generic;
using UnityEngine;

namespace Immersal.Samples.Navigation
{
    public class IsNavigationTarget : MonoBehaviour
    {
        public NavigationTargets.NavigationCategory navigationCategory = NavigationTargets.NavigationCategory.Locations;
        public string targetName;
        public Sprite icon;

        private Collider m_collider = null;
        private Renderer[] m_renderers; // Tambahkan ini untuk handle visual

        public Vector3 position
        {
            get
            {
                if (m_collider != null) return m_collider.bounds.center;
                return transform.position;
            }
            set
            {
                if (m_collider != null)
                {
                    Vector3 currentCenter = m_collider.bounds.center;
                    Vector3 delta = value - currentCenter;
                    transform.position += delta;
                }
                else
                {
                    transform.position = value;
                }
            }
        }

        private void Awake()
        {
            // Ambil semua renderer (mesh, billboard, dll) di objek ini dan anaknya
            m_renderers = GetComponentsInChildren<Renderer>();

            // Sembunyikan visual secara default saat aplikasi mulai
            SetVisible(false);
        }

        private void Start()
        {
            NavigationGraphManager.Instance?.AddTarget(this);

            if (string.IsNullOrEmpty(targetName))
            {
                targetName = gameObject.name;
            }
        }

        // Fungsi baru untuk kontrol muncul/hilang
        public void SetVisible(bool visible)
        {
            // Nyalakan/matikan seluruh child GameObject visual di bawah target ini
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(visible);
            }

            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in allRenderers)
            {
                r.enabled = visible;
            }

            if (m_collider != null) m_collider.enabled = visible;
        }

        private void OnEnable()
        {
            if (m_collider == null)
                m_collider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();

            // Cek agar objek Phantom tidak pernah masuk ke dalam daftar target navigasi
            if (gameObject.name.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                (!string.IsNullOrEmpty(targetName) && targetName.IndexOf("Phantom", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return;
            }

            if (!NavigationTargets.NavigationTargetsDict.ContainsKey(navigationCategory))
                NavigationTargets.NavigationTargetsDict[navigationCategory] = new List<GameObject>();

            if (!NavigationTargets.NavigationTargetsDict[navigationCategory].Contains(gameObject))
                NavigationTargets.NavigationTargetsDict[navigationCategory].Add(gameObject);
        }

        private void OnDisable()
        {
            if (NavigationTargets.NavigationTargetsDict.ContainsKey(navigationCategory))
                NavigationTargets.NavigationTargetsDict[navigationCategory].Remove(gameObject);
        }

        private void OnDestroy()
        {
            if (NavigationGraphManager.HasInstance)
            {
                NavigationGraphManager.Instance.RemoveTarget(this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_collider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        }
#endif
    }
}