using UnityEngine;

/// <summary>
/// 카메라 ortho size를 화면 aspect에 따라 동적 조정 — 모든 화면 비율에서
/// 24 lane (가로 폭 targetWorldWidth) 이 화면 안에 fit.
/// Steelman Scenario C 차단 (Sprint 2 deferred 항목 미리 당김).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAspectFit : MonoBehaviour {
    [Tooltip("가시 영역에 들어와야 할 월드 가로 폭 (24 lane × LANE_WIDTH)")]
    public float targetWorldWidth = 24f;
    [Tooltip("세로 fit 최소값 — 노트 trajectory가 화면 위 아래로 충분히 보여야 함")]
    public float minOrthoSize = 10f;

    private Camera cam;

    void Awake()   { cam = GetComponent<Camera>(); Fit(); }
    void OnEnable(){ if (cam == null) cam = GetComponent<Camera>(); Fit(); }
    void Update()  { Fit(); }

    void Fit() {
        if (cam == null || !cam.orthographic) return;
        float requiredOrthoSize = targetWorldWidth * 0.5f / cam.aspect;
        cam.orthographicSize = Mathf.Max(minOrthoSize, requiredOrthoSize);
    }
}
