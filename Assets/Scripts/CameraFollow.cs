using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float delay = 1;
    Vector3 offset;
    Vector3 nextPos;
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
        nextPos = transform.position;
        var token = this.GetCancellationTokenOnDestroy();
        _ = CameraFollowing(token);
    }
    private async UniTaskVoid CameraFollowing(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            if(!target.IsDestroyed())nextPos = target.position + offset;
            if(!token.IsCancellationRequested) transform.position = transform.position + (nextPos - transform.position) * UnityEngine.UIElements.Experimental.Easing.OutElastic(Time.deltaTime / delay);
        }
    }
}
