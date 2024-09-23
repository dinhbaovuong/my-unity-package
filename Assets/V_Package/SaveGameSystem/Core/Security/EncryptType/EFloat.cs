using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;
using VPackage.SaveGameSystem.Security.EmbedAntiCheat;

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !ENABLE_IL2CPP
#define ACTK_UWP_NO_IL2CPP
#endif

namespace VPackage.SaveGameSystem.Security
{
    [Serializable]
    public struct EFloat : IObscuredType, IFormattable, IEquatable<EFloat>, IComparable<EFloat>, IComparable<float>, IComparable
    {
        //private ObscuredFloat obscured;

        public float Value
        {
            get => InternalDecrypt();
            set
            {
	            currentCryptoKey = GenerateKey();
	            hiddenValue = Encrypt(value, currentCryptoKey);
	            hiddenValueOldByte4 = default(ACTkByte4);

#if UNITY_EDITOR
	            migratedVersion = null;
#endif
	            inited = true;
            }
        }
        
        public EFloat(float value)
        {
            currentCryptoKey = GenerateKey();
            hiddenValue = Encrypt(value, currentCryptoKey);
            hiddenValueOldByte4 = default(ACTkByte4);

#if UNITY_EDITOR
	        migratedVersion = null;
#endif
            inited = true;
        }
        
        
        
        
        
        
        
        //Embed
#if UNITY_EDITOR
        public string migratedVersion;
#endif
        [SerializeField]
        private int currentCryptoKey;

        [SerializeField]
        private int hiddenValue;

        [SerializeField]
        [FormerlySerializedAs("hiddenValue")]
#pragma warning disable 414
        private ACTkByte4 hiddenValueOldByte4;
#pragma warning restore 414

        [SerializeField]
        private bool inited;
        
        
        

        /// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static int Encrypt(float value, int key)
		{
			return FloatIntBytesUnion.XorFloatToInt(value, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static float Decrypt(int value, int key)
		{
			return FloatIntBytesUnion.XorIntToFloat(value, key);
		}

		/// <summary>
		/// Allows to update the raw encrypted value to the newer encryption format.
		/// </summary>
		/// Use when you have some encrypted values saved somewhere with previous ACTk version
		/// and you wish to set them using SetEncrypted() to the newer ACTk version obscured type.
		/// Current migration variants:
		/// from 0 or 1 to 2 - migrate obscured type from ACTk 1.5.2.0-1.5.8.0 to the 1.5.9.0+ format
		/// <param name="encrypted">Encrypted value you got from previous ACTk version obscured type with GetEncrypted().</param>
		/// <param name="fromVersion">Source format version.</param>
		/// <param name="toVersion">Target format version.</param>
		/// <returns>Migrated raw encrypted value which you may use for SetEncrypted(0 later.</returns>
		public static int MigrateEncrypted(int encrypted, byte fromVersion = 0, byte toVersion = 2)
		{
			return FloatIntBytesUnion.Migrate(encrypted, fromVersion, toVersion);
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
		public static EFloat FromEncrypted(int encrypted, int key)
		{
			var instance = new EFloat();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static int GenerateKey()
		{
			return RandomUtils.GenerateIntKey();
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// \sa FromEncrypted(), SetEncrypted()
		public int GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// \sa FromEncrypted()
		public void SetEncrypted(int encrypted, int key)
		{
			inited = true;
			hiddenValue = encrypted;
			currentCryptoKey = key;
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public float GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private float InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(0, currentCryptoKey);
				inited = true;

				return 0;
			}

#if ACTK_OBSCURED_AUTO_MIGRATION
			if (hiddenValueOldByte4.b1 != 0 ||
			    hiddenValueOldByte4.b2 != 0 ||
				hiddenValueOldByte4.b3 != 0 ||
				hiddenValueOldByte4.b4 != 0)
			{
				var union = new FloatIntBytesUnion {b4 = hiddenValueOldByte4};
				union.b4.Shuffle();
				hiddenValue = union.i;

				hiddenValueOldByte4.b1 = 0;
				hiddenValueOldByte4.b2 = 0;
				hiddenValueOldByte4.b3 = 0;
				hiddenValueOldByte4.b4 = 0;
			}
#endif

			var decrypted = Decrypt(hiddenValue, currentCryptoKey);
			
			return decrypted;
		}

		#region operators, overrides, interface implementations

		private static EFloat Increment(EFloat input, int increment)
		{
			var decrypted = input.InternalDecrypt() + increment;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			return input;
		}

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
			return obj is EFloat && Equals((EFloat)obj);
		}

		public bool Equals(EFloat obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}

		public int CompareTo(EFloat other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(float other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
#if !ACTK_UWP_NO_IL2CPP
			return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is float)) throw new ArgumentException("Argument must be float");
			return CompareTo((float)obj);
#endif
		}

		#endregion

		#region obsolete

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.")]
		public static void SetNewCryptoKey(int newKey) {}

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.")]
		public void ApplyNewCryptoKey() {}

		[Obsolete("Please use new Encrypt(value, key) API instead.", true)]
		public static int Encrypt(float value) { throw new Exception(); }

		[Obsolete("Please use new Decrypt(value, key) API instead.", true)]
		public static float Decrypt(int value) { throw new Exception(); }

		[Obsolete("Please use new FromEncrypted(encrypted, key) API instead.", true)]
		public static EFloat FromEncrypted(int encrypted) { throw new Exception(); }

		[Obsolete("Please use new GetEncrypted(out key) API instead.", true)]
		public int GetEncrypted() { throw new Exception(); }

		[Obsolete("Please use new SetEncrypted(encrypted, key) API instead.", true)]
		public void SetEncrypted(int encrypted) {}

		#endregion

		//! @endcond

		[StructLayout(LayoutKind.Explicit)]
		internal struct FloatIntBytesUnion
		{
			[FieldOffset(0)]
			internal float f;

			[FieldOffset(0)]
			internal int i;

			[FieldOffset(0)]
			internal ACTkByte4 b4;

			public static int Migrate(int value, byte fromVersion, byte toVersion)
			{
				var u = FromInt(value);

				if (fromVersion < 2 && toVersion == 2)
				{
					u.b4.Shuffle();
				}

				return u.i;
			}

			internal static int XorFloatToInt(float value, int key)
			{
				return FromFloat(value).Shuffle(key).i;
			}

			internal static float XorIntToFloat(int value, int key)
			{
				return FromInt(value).UnShuffle(key).f;
			}

			private static FloatIntBytesUnion FromFloat(float value)
			{
				return new FloatIntBytesUnion { f = value};
			}

			private static FloatIntBytesUnion FromInt(int value)
			{
				return new FloatIntBytesUnion { i = value};
			}

			private FloatIntBytesUnion Shuffle(int key)
			{
				i ^= key;
				b4.Shuffle();

				return this;
			}

			private FloatIntBytesUnion UnShuffle(int key)
			{
				b4.UnShuffle();
				i ^= key;

				return this;
			}
		}
    }
}