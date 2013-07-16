

Texture2D texBase : register(t0);
Texture2D texBlend : register(t1);

RWTexture2D<float4> rwtexOutput : register(u0);

#define thread_x 1
#define thread_y 1


#define begin(Name) \
	[numthreads(thread_x, thread_y, 1)] \
	void Name(uint3 _tid : SV_DispatchThreadID) \
	{ \
		int3 _pos = int3(_tid.x, _tid.y, 0); \
		float4 baseColor = texBase.Load(_pos); \
		float4 blendColor = texBlend.Load(_pos); \
		float4 result = float4(0, 0, 0, 0);

#define end \
		rwtexOutput[uint2(_tid.x, _tid.y)] = result; \
	}



float _luma(float4 color)
{
	return color.r * 0.29891 + color.g * 0.58661 + color.b * 0.11448;
}

float _noise(uint seed)
{
	// http://fssnip.net/gy
	seed = (seed ^ 61) ^ (seed >> 16);
	seed *= 9;
	seed = seed ^ (seed >> 4);
	seed *= 0x27d4eb2d;
	seed = seed ^ (seed >> 15);
	return seed / 4294967296.0;
}


// ???
begin(PassThrough)
end

// invalid composition
begin(Unknown)
	result = float4(0, 0, 0, 0);
end



//***************************************
// 通常合成
//***************************************

// 通常
begin(Normal)
	result = baseColor * (1.0 - blendColor.a) + blendColor * blendColor.a;
end

// ディザ合成
begin(Dissolve)
	// blendColor.a と「baseColor が上書きされる確率」が比例する
	float n = _noise(_tid.x*1920 + _tid.y);
	result = ((n + 1.0) < blendColor.a * 2.0) ? blendColor : baseColor;
end
//***************************************



//***************************************
// 暗くする合成
//***************************************

// 比較（暗）
begin(Darken)
end

// 乗算
begin(Multiply)
	result = baseColor * blendColor;
end

// 焼き込みカラー
begin(ColorBurn)
end

// 焼き込み（リニア）
begin(LinearBurn)
end

// カラー比較（暗）
begin(DarkerColor)
end
//***************************************



//***************************************
// 明るくする合成
//***************************************

// 比較（明）
begin(Lighten)
end

// スクリーン
begin(Screen)
	//result = 1.0 - (1.0 - baseColor) * (1.0 - blendColor);
	result = baseColor + blendColor - (baseColor * blendColor);
end

// 覆い焼きカラー
begin(ColorDodge)
end

// 覆い焼き（リニア）- 加算
begin(LinearDodge)
	result = baseColor + blendColor;
end

// カラー比較（明）
begin(LighterColor)
end
//***************************************



//***************************************
// コントラストを変える合成
//***************************************

// オーバーレイ
begin(Overlay)
	// 暗いところはより暗く、明るいところはより明るく
	// baseLuma < 0.5 ? Multiply*2 : Screen*2
	float baseL = _luma(baseColor);
	if(baseL < 0.5)
	{
		result = baseColor * blendColor * 2.0;
	}
	else
	{
		// result = 1.0 - 2.0 * (1.0 - baseColor) * (1.0 - blendColor);
		result = (baseColor + blendColor - (baseColor * blendColor)) * 2.0 - 1.0;
	}
end

// ソフトライト
begin(SoftLight)
	// ハードライトを柔らかくしたやつ
	float blendL = _luma(blendColor);
	if(blendL < 0.5)
	{
		//result = baseColor * blendColor * 2.0 + baseColor * baseColor * (1.0 - blendColor * 2.0);
		result = baseColor * (blendColor * 2.0 + baseColor * (1.0 - blendColor * 2.0));
	}
	else
	{
		result = baseColor * 2.0 * (1.0 - blendColor) + sqrt(baseColor) * (blendColor * 2.0 - 1.0);
	}
end

// ハードライト
begin(HardLight)
	// オーバーレイの逆
	float baseL = _luma(baseColor);
	if(baseL < 0.5)
	{
		result = blendColor * baseColor * 2.0;
	}
	else
	{
		result = (blendColor + baseColor - (blendColor * baseColor)) * 2.0 - 1.0;
	}
end

// ビビッドライト
begin(VividLight)
end

// リニアライト
begin(LinearLight)
end

// ピンライト
begin(PinLight)
end

// ハードミックス
begin(HardMix)
end
//***************************************



//***************************************
// 色を反転させる合成
//***************************************

// 差の絶対値
begin(Difference)
end

// 除外
begin(Exclusion)
end
//***************************************



//***************************************
// 色の値を小さくする合成
//***************************************

// 減算
begin(Subtract)
end

// 除算
begin(Divide)
end
//***************************************



//***************************************
// HSL+Color の合成
//***************************************

// 色相
begin(Hue)
end

// 彩度
begin(Saturation)
end

// カラー
begin(Color)
end

// 輝度
begin(Luminosity)
end
//***************************************



