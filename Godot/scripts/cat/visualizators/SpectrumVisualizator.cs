using Godot;
using System;

public partial class SpectrumVisualizator : TextureRect
{
	[Export]
	public float MaxHZ = 60000;
	[Export]
	public float MinHZ = 60;
	[Export(PropertyHint.ExpEasing)]
	public float SpectrumScale = 1;
	[Export]
	public float SpectrumPower = 1f;

	private AudioEffectSpectrumAnalyzerInstance Spectrum = AudioServer.GetBusEffectInstance(2, 1) as AudioEffectSpectrumAnalyzerInstance;
	
	public override void _Process(double delta)
	{
		if (IsVisibleInTree())
		{
			float[] values = new float[1024];
			for (int i = 0; i < values.Length; i++)
			{
				Vector2 specPart = 
				Spectrum.GetMagnitudeForFrequencyRange(
					MathF.Pow((float)i / values.Length, SpectrumScale) * MaxHZ,
					MathF.Pow((i + 1f) / values.Length, SpectrumScale) * MaxHZ,
					AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Max
				) * SpectrumPower;
				values[i] = specPart.X;
			}
			(Material as ShaderMaterial).SetShaderParameter("current_values", values);
		}
	}
}
