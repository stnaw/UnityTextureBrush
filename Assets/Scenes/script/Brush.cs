using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
struct BrushJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Color32> ReadColors;
    [WriteOnly] public NativeArray<Color32> WriteColors;
    [ReadOnly]  public Vector4 PaintRange;
    [ReadOnly]  public Vector2Int SourceTextureSize;
    [ReadOnly]  public Vector2 CenterPoint;
    Vector2 clamp(Vector2 min, Vector2 max, Vector2 current)
    {
        current.x = Mathf.Max(min.x, Mathf.Min(max.x, current.x));
        current.y = Mathf.Max(min.y, Mathf.Min(max.y, current.y));
        return current;
    }
    public void Execute(int index)
    {
        int row = index / SourceTextureSize.x;
        int col = index % SourceTextureSize.x;
        var max = 50f;

        if(row > PaintRange.y && row < PaintRange.w && col > PaintRange.x && col < PaintRange.z)
        {
            var distance = Vector2.Distance(new Vector2(col, row), CenterPoint);
            Color32 org = ReadColors[index];
            org = distance > max ? org : new Color32(255, 0, 0, 255);
            WriteColors[index] = org;
        }
    }
}

public class Brush
{
    private Texture2D targetTexture = null;
    private NativeArray<Color32> targetTextureColors;

    private Vector2 texSize = Vector2.zero;
    public Brush(Texture2D targetTex)
    {
        targetTexture = targetTex;
        targetTextureColors = targetTex.GetRawTextureData<Color32>();
        texSize.x = targetTex.width;
        texSize.y = targetTex.height;
    }

    Vector2 clamp(Vector2 min, Vector2 max, Vector2 current)
    {
        current.x = Mathf.Max(min.x, Mathf.Min(max.x, current.x));
        current.y = Mathf.Max(min.y, Mathf.Min(max.y, current.y));
        return current;
    }

    public void Draw(Vector2 point)
    {
        // this.draw_jobsystem(point);
        this.draw_cpu(point);
    }

    private void draw_jobsystem(Vector2 point)
    {
        Vector2 brushSize = new Vector2(100, 100);
        Vector2 start = clamp(Vector2.zero, texSize, point - brushSize * 0.5f);
        Vector2 end = clamp(Vector2.zero, texSize, point + brushSize * 0.5f);
        NativeArray<Color32> temp = new NativeArray<Color32>(targetTextureColors, Allocator.TempJob);
        var job = new BrushJob()
        {
            ReadColors = temp,
            WriteColors = targetTextureColors,
            PaintRange = new Vector4(start.x, start.y, end.x, end.y),
            SourceTextureSize = new Vector2Int((int)texSize.x, (int)texSize.y),
            CenterPoint = point
        };
        var job_handle = job.Schedule(targetTextureColors.Length, 64);
        job_handle.Complete();
        temp.Dispose();
    }

    private void draw_cpu(Vector2 point)
    {
        Vector2 brushSize = new Vector2(50, 50);
        Vector2 start = clamp(Vector2.zero, texSize, point - brushSize * 0.5f);
        Vector2 end = clamp(Vector2.zero, texSize, point + brushSize * 0.5f);
        var max = 25f;

        for (int i = (int)start.x; i < end.x; i++)
        {
            for (int j = (int)start.y; j < end.y; j++)
            {
                var distance = Vector2.Distance(new Vector2(i, j), point);
                int index = j * targetTexture.width + i;
                Color32 org = targetTextureColors[index];
                org = distance > max ? org : new Color32(255, 0, 0, 255);
                targetTextureColors[index] = org;
            }
        }
    }

    public void Apply()
    {
        targetTexture.Apply();
    }



}
