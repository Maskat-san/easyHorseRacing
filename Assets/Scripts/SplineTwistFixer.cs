using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineTwistFixer : MonoBehaviour
{
    public SplineContainer splineContainer;

    // コンテキストメニューから実行できるようにする魔法の属性
    [ContextMenu("Fix Spline Twist (馬場のねじれを直す)")]
    public void FixTwist()
    {
        // アタッチされているSplineContainerを自動取得
        if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null) return;

        var splines = splineContainer.Splines;
        for (int s = 0; s < splines.Count; s++)
        {
            var spline = splines[s];
            for (int i = 0; i < spline.Count; i++)
            {
                BezierKnot knot = spline[i];

                // そのKnotの進行方向ベクトルを取得
                float3 tangent = knot.TangentOut;
                // もし進行方向が0なら適当に前を向かせる
                if (math.lengthsq(tangent) == 0f) tangent = new float3(0, 0, 1);

                // 【最重要】進行方向と「ワールドの真上(math.up)」を使って、ねじれのない正しい回転を計算
                quaternion flatRotation = quaternion.LookRotationSafe(tangent, math.up());

                // 計算した「真っ平らな回転」をKnotに上書き！
                knot.Rotation = flatRotation;
                spline[i] = knot;
            }
        }

        Debug.Log("馬場のねじれを一掃しました！");
    }
}
