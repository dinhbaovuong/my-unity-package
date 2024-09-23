using System;
using System.Runtime.InteropServices;
using UnityEngine;
using VPackage.SaveGameSystem.Security.EmbedAntiCheat;

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

namespace VPackage.SaveGameSystem.Security
{
	/// <summary>
	/// Use it instead of regular <c>decimal</c> for any cheating-sensitive variables.
	/// </summary>
	/// <strong><em>Regular type is faster and memory wiser comparing to the obscured one!</em></strong><br/>
	/// Feel free to use regular types for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).
	[Serializable]
	public struct EDecimal : IObscuredType, IFormattable, IEquatable<EDecimal>, IComparable<EDecimal>, IComparable<decimal>, IComparable
	{
		public decimal Value
		{
			get => InternalDecrypt();
			set
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = InternalEncrypt(value, currentCryptoKey);
				
				inited = true;
			}
		}

		public EDecimal(decimal value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = InternalEncrypt(value, currentCryptoKey);
			
			inited = true;
		}
		
		
		
		
		
		
		//Embed
		[SerializeField]
		private long currentCryptoKey;

		[SerializeField]
		private ACTkByte16 hiddenValue;

		[SerializeField]
		private bool inited;
		
		

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static decimal Encrypt(decimal value, long key)
		{
			return DecimalLongBytesUnion.XorDecimalToDecimal(value, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static decimal Decrypt(decimal value, long key)
		{
			return DecimalLongBytesUnion.XorDecimalToDecimal(value, key);
		}

		/// <summary>
		/// Creates and fills obscured variable with raw encrypted value previously got from GetEncrypted().
		/// </summary>
		/// Literally does same job as SetEncrypted() but makes new instance instead of filling existing one,
		/// making it easier to initialize new variables from saved encrypted values.
		///
		/// <param name="encrypted">Raw encrypted value you got from GetEncrypted().</param>
		/// <param name="key">Encryption key you've got from GetEncrypted().</param>
		/// <returns>New obscured variable initialized from specified encrypted value.</returns>
		/// \sa GetEncrypted(), SetEncrypted()
		public static EDecimal FromEncrypted(decimal encrypted, long key)
		{
			var instance = new EDecimal();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static long GenerateKey()
		{
			return RandomUtils.GenerateLongKey();
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// \sa FromEncrypted(), SetEncrypted()
		public decimal GetEncrypted(out long key)
		{
			if (!inited)
				Init();
			
			key = currentCryptoKey;
			return DecimalLongBytesUnion.ConvertB16ToDecimal(hiddenValue);
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// \sa FromEncrypted()
		public void SetEncrypted(decimal encrypted, long key)
		{
			inited = true;
			hiddenValue = DecimalLongBytesUnion.ConvertDecimalToB16(encrypted);
			currentCryptoKey = key;
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public decimal GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		private static ACTkByte16 InternalEncrypt(decimal value, long key)
		{
			return DecimalLongBytesUnion.XorDecimalToB16(value, key);
		}

		private decimal InternalDecrypt()
		{
			if (!inited)
			{
				Init();
				return 0m;
			}

			var decrypted = DecimalLongBytesUnion.XorB16ToDecimal(hiddenValue, currentCryptoKey);
			return decrypted;
		}
		
		private void Init()
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = InternalEncrypt(0m, currentCryptoKey);
			inited = true;
		}
		
		#region operators, overrides, interface implementations

		//! @cond
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}

		public override bool Equals(object obj)
		{
			return obj is EDecimal && Equals((EDecimal)obj);
		}

		public bool Equals(EDecimal obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}

		public int CompareTo(EDecimal other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(decimal other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
#if !ACTK_UWP_NO_IL2CPP
			return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is decimal)) throw new ArgumentException("Argument must be decimal");
			return CompareTo((decimal)obj);
#endif
		}

		#endregion

		//! @endcond

		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalLongBytesUnion
		{
			[FieldOffset(0)]
			private decimal d;

			[FieldOffset(0)]
			private long l1;

			[FieldOffset(8)]
			private long l2;

			[FieldOffset(0)]
			private ACTkByte16 b16;

			internal static decimal XorDecimalToDecimal(decimal value, long key)
			{
				return FromDecimal(value).XorLongs(key).d;
			}

			internal static ACTkByte16 XorDecimalToB16(decimal value, long key)
			{
				return FromDecimal(value).XorLongs(key).b16;
			}

			internal static decimal XorB16ToDecimal(ACTkByte16 value, long key)
			{
				return FromB16(value).XorLongs(key).d;
			}

			internal static decimal ConvertB16ToDecimal(ACTkByte16 value)
			{
				return FromB16(value).d;
			}

			internal static ACTkByte16 ConvertDecimalToB16(decimal value)
			{
				return FromDecimal(value).b16;
			}

			private static DecimalLongBytesUnion FromDecimal(decimal value)
			{
				return new DecimalLongBytesUnion {d = value};
			}

			private static DecimalLongBytesUnion FromB16(ACTkByte16 value)
			{
				return new DecimalLongBytesUnion {b16 = value};
			}

			private DecimalLongBytesUnion XorLongs(long key)
			{
				l1 ^= key;
				l2 ^= key;
				return this;
			}
		}
	}
}