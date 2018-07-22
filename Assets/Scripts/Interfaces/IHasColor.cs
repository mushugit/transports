using UnityEngine;

public interface IHasColor
{
    float ColorR { get; }
    float ColorG { get; }
    float ColorB { get; }
    float ColorA { get; }
    void SetColor(Color color);
    Color Color { get; }
}

