using System;
using UnityEngine;

public class vp_SmoothRandom
{
	private static vp_FractalNoise s_Noise;

	public static Vector3 GetVector3(float speed)
	{
		float x = Time.time * 0.01f * speed;
		return new Vector3(Get().HybridMultifractal(x, 15.73f, 0.58f), Get().HybridMultifractal(x, 63.94f, 0.58f), Get().HybridMultifractal(x, 0.2f, 0.58f));
	}

	public static Vector3 GetVector3Centered(float speed)
	{
		float x = Time.time * 0.01f * speed;
		float x2 = (Time.time - 1f) * 0.01f * speed;
		Vector3 vector = new Vector3(Get().HybridMultifractal(x, 15.73f, 0.58f), Get().HybridMultifractal(x, 63.94f, 0.58f), Get().HybridMultifractal(x, 0.2f, 0.58f));
		Vector3 vector2 = new Vector3(Get().HybridMultifractal(x2, 15.73f, 0.58f), Get().HybridMultifractal(x2, 63.94f, 0.58f), Get().HybridMultifractal(x2, 0.2f, 0.58f));
		return vector - vector2;
	}

	public static float Get(float speed)
	{
		float num = Time.time * 0.01f * speed;
		return Get().HybridMultifractal(num * 0.01f, 15.7f, 0.65f);
	}

	private static vp_FractalNoise Get()
	{
		if (s_Noise == null)
		{
			s_Noise = new vp_FractalNoise(1.27f, 2.04f, 8.36f);
		}
		return s_Noise;
	}
}

public class vp_Perlin
{
	private const int B = 256;

	private const int BM = 255;

	private const int N = 4096;

	private int[] p = new int[514];

	private float[,] g3 = new float[514, 3];

	private float[,] g2 = new float[514, 2];

	private float[] g1 = new float[514];

	public vp_Perlin()
	{
		System.Random random = new System.Random();
		int i;
		for (i = 0; i < 256; i++)
		{
			p[i] = i;
			g1[i] = (float)(random.Next(512) - 256) / 256f;
			for (int j = 0; j < 2; j++)
			{
				g2[i, j] = (float)(random.Next(512) - 256) / 256f;
			}
			normalize2(ref g2[i, 0], ref g2[i, 1]);
			for (int j = 0; j < 3; j++)
			{
				g3[i, j] = (float)(random.Next(512) - 256) / 256f;
			}
			normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
		}
		while (--i != 0)
		{
			int num = p[i];
			int j;
			p[i] = p[j = random.Next(256)];
			p[j] = num;
		}
		for (i = 0; i < 258; i++)
		{
			p[256 + i] = p[i];
			g1[256 + i] = g1[i];
			for (int j = 0; j < 2; j++)
			{
				g2[256 + i, j] = g2[i, j];
			}
			for (int j = 0; j < 3; j++)
			{
				g3[256 + i, j] = g3[i, j];
			}
		}
	}

	private float s_curve(float t)
	{
		return t * t * (3f - 2f * t);
	}

	private float lerp(float t, float a, float b)
	{
		return a + t * (b - a);
	}

	private void setup(float value, out int b0, out int b1, out float r0, out float r1)
	{
		float num = value + 4096f;
		b0 = (int)num & 0xFF;
		b1 = (b0 + 1) & 0xFF;
		r0 = num - (float)(int)num;
		r1 = r0 - 1f;
	}

	private float at2(float rx, float ry, float x, float y)
	{
		return rx * x + ry * y;
	}

	private float at3(float rx, float ry, float rz, float x, float y, float z)
	{
		return rx * x + ry * y + rz * z;
	}

	public float Noise(float arg)
	{
		int b;
		int b2;
		float r;
		float r2;
		setup(arg, out b, out b2, out r, out r2);
		float t = s_curve(r);
		float a = r * g1[p[b]];
		float b3 = r2 * g1[p[b2]];
		return lerp(t, a, b3);
	}

	public float Noise(float x, float y)
	{
		int b;
		int b2;
		float r;
		float r2;
		setup(x, out b, out b2, out r, out r2);
		int b3;
		int b4;
		float r3;
		float r4;
		setup(y, out b3, out b4, out r3, out r4);
		int num = p[b];
		int num2 = p[b2];
		int num3 = p[num + b3];
		int num4 = p[num2 + b3];
		int num5 = p[num + b4];
		int num6 = p[num2 + b4];
		float t = s_curve(r);
		float t2 = s_curve(r3);
		float a = at2(r, r3, g2[num3, 0], g2[num3, 1]);
		float b5 = at2(r2, r3, g2[num4, 0], g2[num4, 1]);
		float a2 = lerp(t, a, b5);
		a = at2(r, r4, g2[num5, 0], g2[num5, 1]);
		b5 = at2(r2, r4, g2[num6, 0], g2[num6, 1]);
		float b6 = lerp(t, a, b5);
		return lerp(t2, a2, b6);
	}

	public float Noise(float x, float y, float z)
	{
		int b;
		int b2;
		float r;
		float r2;
		setup(x, out b, out b2, out r, out r2);
		int b3;
		int b4;
		float r3;
		float r4;
		setup(y, out b3, out b4, out r3, out r4);
		int b5;
		int b6;
		float r5;
		float r6;
		setup(z, out b5, out b6, out r5, out r6);
		int num = p[b];
		int num2 = p[b2];
		int num3 = p[num + b3];
		int num4 = p[num2 + b3];
		int num5 = p[num + b4];
		int num6 = p[num2 + b4];
		float t = s_curve(r);
		float t2 = s_curve(r3);
		float t3 = s_curve(r5);
		float a = at3(r, r3, r5, g3[num3 + b5, 0], g3[num3 + b5, 1], g3[num3 + b5, 2]);
		float b7 = at3(r2, r3, r5, g3[num4 + b5, 0], g3[num4 + b5, 1], g3[num4 + b5, 2]);
		float a2 = lerp(t, a, b7);
		a = at3(r, r4, r5, g3[num5 + b5, 0], g3[num5 + b5, 1], g3[num5 + b5, 2]);
		b7 = at3(r2, r4, r5, g3[num6 + b5, 0], g3[num6 + b5, 1], g3[num6 + b5, 2]);
		float b8 = lerp(t, a, b7);
		float a3 = lerp(t2, a2, b8);
		a = at3(r, r3, r6, g3[num3 + b6, 0], g3[num3 + b6, 2], g3[num3 + b6, 2]);
		b7 = at3(r2, r3, r6, g3[num4 + b6, 0], g3[num4 + b6, 1], g3[num4 + b6, 2]);
		a2 = lerp(t, a, b7);
		a = at3(r, r4, r6, g3[num5 + b6, 0], g3[num5 + b6, 1], g3[num5 + b6, 2]);
		b7 = at3(r2, r4, r6, g3[num6 + b6, 0], g3[num6 + b6, 1], g3[num6 + b6, 2]);
		b8 = lerp(t, a, b7);
		float b9 = lerp(t2, a2, b8);
		return lerp(t3, a3, b9);
	}

	private void normalize2(ref float x, ref float y)
	{
		float num = (float)Math.Sqrt(x * x + y * y);
		x = y / num;
		y /= num;
	}

	private void normalize3(ref float x, ref float y, ref float z)
	{
		float num = (float)Math.Sqrt(x * x + y * y + z * z);
		x = y / num;
		y /= num;
		z /= num;
	}
}

public class vp_FractalNoise
{
	private vp_Perlin m_Noise;

	private float[] m_Exponent;

	private int m_IntOctaves;

	private float m_Octaves;

	private float m_Lacunarity;

	public vp_FractalNoise(float inH, float inLacunarity, float inOctaves)
		: this(inH, inLacunarity, inOctaves, null)
	{
	}

	public vp_FractalNoise(float inH, float inLacunarity, float inOctaves, vp_Perlin noise)
	{
		m_Lacunarity = inLacunarity;
		m_Octaves = inOctaves;
		m_IntOctaves = (int)inOctaves;
		m_Exponent = new float[m_IntOctaves + 1];
		float num = 1f;
		for (int i = 0; i < m_IntOctaves + 1; i++)
		{
			m_Exponent[i] = (float)Math.Pow(m_Lacunarity, 0f - inH);
			num *= m_Lacunarity;
		}
		if (noise == null)
		{
			m_Noise = new vp_Perlin();
		}
		else
		{
			m_Noise = noise;
		}
	}

	public float HybridMultifractal(float x, float y, float offset)
	{
		float num = (m_Noise.Noise(x, y) + offset) * m_Exponent[0];
		float num2 = num;
		x *= m_Lacunarity;
		y *= m_Lacunarity;
		int i;
		for (i = 1; i < m_IntOctaves; i++)
		{
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			float num3 = (m_Noise.Noise(x, y) + offset) * m_Exponent[i];
			num += num2 * num3;
			num2 *= num3;
			x *= m_Lacunarity;
			y *= m_Lacunarity;
		}
		float num4 = m_Octaves - (float)m_IntOctaves;
		return num + num4 * m_Noise.Noise(x, y) * m_Exponent[i];
	}

	public float RidgedMultifractal(float x, float y, float offset, float gain)
	{
		float num = Mathf.Abs(m_Noise.Noise(x, y));
		num = offset - num;
		num *= num;
		float num2 = num;
		float num3 = 1f;
		for (int i = 1; i < m_IntOctaves; i++)
		{
			x *= m_Lacunarity;
			y *= m_Lacunarity;
			num3 = num * gain;
			num3 = Mathf.Clamp01(num3);
			num = Mathf.Abs(m_Noise.Noise(x, y));
			num = offset - num;
			num *= num;
			num *= num3;
			num2 += num * m_Exponent[i];
		}
		return num2;
	}

	public float BrownianMotion(float x, float y)
	{
		float num = 0f;
		long num2;
		for (num2 = 0L; num2 < m_IntOctaves; num2++)
		{
			num = m_Noise.Noise(x, y) * m_Exponent[num2];
			x *= m_Lacunarity;
			y *= m_Lacunarity;
		}
		float num3 = m_Octaves - (float)m_IntOctaves;
		return num + num3 * m_Noise.Noise(x, y) * m_Exponent[num2];
	}
}
