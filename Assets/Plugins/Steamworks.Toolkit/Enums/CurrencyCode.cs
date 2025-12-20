#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UnityEngine;

namespace Steamworks.Toolkit.Enums
{
	[SuppressMessage( "ReSharper", "InconsistentNaming" )]
	public enum ECurrencyCode
	{
		Unknown,
		AED,
		ARS,
		AUD,
		BRL,
		CAD,
		CHF,
		CLP,
		CNY,
		COP,
		CRC,
		EUR,
		GBP,
		HKD,
		ILS,
		IDR,
		INR,
		JPY,
		KRW,
		KWD,
		KZT,
		MXN,
		MYR,
		NOK,
		NZD,
		PEN,
		PHP,
		PLN,
		QAR,
		RUB,
		SAR,
		SGD,
		THB,
		TRY,
		TWD,
		UAH,
		USD,
		UYU,
		VND,
		ZAR
	}

	public static class CurrencyCode
	{
		[RuntimeInitializeOnLoadMethod]
		public static void InitializeOnLoad( )
		{
			SteamCallResultObserver<SteamInventoryRequestPricesResult_t>.OnTrigger += ( response, ioError ) =>
			{
				if( ioError || response.m_result != EResult.k_EResultOK )
					Debug.LogWarning( "Failed to fetch current prices for the list of available inventory items.\nSteam Response: " + response.m_result.ToString( ) );
				else
					LocalCurrencyCode = (ECurrencyCode)Enum.Parse( typeof(ECurrencyCode), response.m_rgchCurrency.ToUpper( ) );
			};
		}

		public static ECurrencyCode LocalCurrencyCode   { get; private set; }
		public static String        LocalCurrencySymbol => GetSymbol( LocalCurrencyCode );
		public static NumberFormatInfo CurrencyFormat
		{
			get
			{
				var culture = (CultureInfo)CultureInfo.GetCultureInfo( LocalCurrencyCode.GetCultureForCode( ) ).Clone( );

				var format = culture.NumberFormat;
				format.CurrencySymbol = LocalCurrencySymbol;

				return format;
			}
		}
		public static String GetSymbol( this ECurrencyCode currencyCode )
		{
			switch( currencyCode )
			{
				case ECurrencyCode.Unknown: return String.Empty;

				case ECurrencyCode.AED: return "د.إ";

				case ECurrencyCode.BRL: return "R$";

				case ECurrencyCode.CHF: return "CHF";

				case ECurrencyCode.CNY: return "¥";

				case ECurrencyCode.CRC: return "₡";

				case ECurrencyCode.EUR: return "€";

				case ECurrencyCode.GBP: return "£";

				case ECurrencyCode.ILS: return "₪";

				case ECurrencyCode.IDR: return "Rp";

				case ECurrencyCode.INR: return "₹";

				case ECurrencyCode.JPY: return "¥";

				case ECurrencyCode.KRW: return "₩";

				case ECurrencyCode.KWD: return "د.ك";

				case ECurrencyCode.KZT: return "лв";

				case ECurrencyCode.MYR: return "RM";

				case ECurrencyCode.NOK: return "kr";

				case ECurrencyCode.PEN: return "S/.";

				case ECurrencyCode.PHP: return "₱";

				case ECurrencyCode.PLN: return "zł";

				case ECurrencyCode.QAR: return "﷼";

				case ECurrencyCode.RUB: return "₽";

				case ECurrencyCode.SAR: return "﷼";

				case ECurrencyCode.THB: return "฿";

				case ECurrencyCode.TRY: return "₺";

				case ECurrencyCode.TWD: return "NT$";

				case ECurrencyCode.UAH: return "₴";

				case ECurrencyCode.UYU: return "$U";

				case ECurrencyCode.VND: return "₫";

				case ECurrencyCode.ZAR: return "R";

				default: return "$";
			}
		}

		public static String GetCultureForCode( this ECurrencyCode currencyCode )
		{
			return currencyCode switch
			{
				ECurrencyCode.AED => "ar-AE",
				ECurrencyCode.ARS => "es-AR",
				ECurrencyCode.AUD => "en-AU",
				ECurrencyCode.BRL => "pt-BR",
				ECurrencyCode.CAD => "en-CA",
				ECurrencyCode.CHF => "de-CH",
				ECurrencyCode.CLP => "es-CL",
				ECurrencyCode.CNY => "zh-CN",
				ECurrencyCode.COP => "es-CO",
				ECurrencyCode.CRC => "es-CR",
				ECurrencyCode.EUR => "fr-FR",
				ECurrencyCode.GBP => "en-GB",
				ECurrencyCode.HKD => "zh-HK",
				ECurrencyCode.ILS => "he-IL",
				ECurrencyCode.IDR => "id-ID",
				ECurrencyCode.INR => "hi-IN",
				ECurrencyCode.JPY => "ja-JP",
				ECurrencyCode.KRW => "ko-KR",
				ECurrencyCode.KWD => "ar-KW",
				ECurrencyCode.KZT => "kk-KZ",
				ECurrencyCode.MXN => "es-MX",
				ECurrencyCode.MYR => "ms-MY",
				ECurrencyCode.NOK => "nb-NO",
				ECurrencyCode.NZD => "en-NZ",
				ECurrencyCode.PEN => "es-PE",
				ECurrencyCode.PHP => "en-PH",
				ECurrencyCode.PLN => "pl-PL",
				ECurrencyCode.QAR => "ar-QA",
				ECurrencyCode.RUB => "ru-RU",
				ECurrencyCode.SAR => "ar-SA",
				ECurrencyCode.SGD => "en-SG",
				ECurrencyCode.THB => "th-TH",
				ECurrencyCode.TRY => "tr-TR",
				ECurrencyCode.TWD => "zh-TW",
				ECurrencyCode.UAH => "uk-UA",
				ECurrencyCode.USD => "en-US",
				ECurrencyCode.UYU => "es-UY",
				ECurrencyCode.VND => "vi-VN",
				ECurrencyCode.ZAR => "en-ZA",
				_                 => CultureInfo.InvariantCulture.Name
			};
		}
	}
}
#endif