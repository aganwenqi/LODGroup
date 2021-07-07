using Chess.LODGroupIJob.SpaceManager;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Chess.LODGroupIJob.JobSystem
{
    [BurstCompile(CompileSynchronously = true)]
    public struct LODCalculateJob : IJobParallelFor
    {
        [ReadOnly]
        public bool orthographic;
        [ReadOnly]
        public float orthographicSize;
        [ReadOnly]
        public float fieldOfView;
        [ReadOnly]
        public float lodBias;
        //��������
        [ReadOnly]
        public Vector3 camPosition;
        //centerת����������
        [ReadOnly]
        public NativeArray<Bounds> bounds;
        //lod����ռ��λ��[0-1]
        [ReadOnly]
        public NativeArray<Float8> lodRelatives;
        //�Ƿ����л�lod����
        [ReadOnly]
        public NativeArray<bool> openBuffer;
        //����
        public NativeArray<JobResult> result;
        public void Execute(int index)
        {
            float preRelative;
            QuadTreeSpaceManager.SettingCamera(orthographic, orthographicSize, fieldOfView, lodBias, out preRelative);
            JobResult r = QuadTreeSpaceManager.SettingCameraJob(bounds[index], camPosition, preRelative);

            var lastResult = result[index];
            //������һ�ε�λ�ÿ����Ƿ���Ҫ�л�LOD
            Float8 f8 = lodRelatives[index];
            for (int i = 0; i < 8; i++)
            {
                float lodRelative = 0;
                switch (i)
                {
                    case 0:
                        lodRelative = f8.v0;
                        break;
                    case 1:
                        lodRelative = f8.v1;
                        break;
                    case 2:
                        lodRelative = f8.v2;
                        break;
                    case 3:
                        lodRelative = f8.v3;
                        break;
                    case 4:
                        lodRelative = f8.v4;
                        break;
                    case 5:
                        lodRelative = f8.v5;
                        break;
                    case 6:
                        lodRelative = f8.v6;
                        break;
                    case 7:
                        lodRelative = f8.v7;
                        break;
                }
                if (lodRelative == 0 && i != 0)
                {
                    r.lodLevel = -1;
                    break;
                }
                if (openBuffer[index] && lastResult.lodLevel == i)
                    lodRelative = lodRelative * 0.9f;
                if (r.relative > lodRelative)
                {
                    r.lodLevel = i;
                    break;
                }
            }
            /* if (openBuffer[index] && lastResult.lodLevel < r.lodLevel && lastResult.lodLevel != -1)
             {
                 if (lastResult.relative * 0.9f < r.relative)
                 {
                     r.lodLevel = lastResult.lodLevel;
                     r.relative = lastResult.relative;
                 }
             }*/
            result[index] = r;
        }
    }
}
