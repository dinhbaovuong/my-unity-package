using System;
using UnityEngine;
using VPackage.SaveGameSystem.Security.EmbedAntiCheat;

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !ENABLE_IL2CPP
#define ACTK_UWP_NO_IL2CPP
#endif

namespace VPackage.SaveGameSystem.Security
{
    [Serializable]
    public struct ELong : IObscuredType, IFormattable, IEquatable<ELong>, IComparable<ELong>, IComparable<long>, IComparable
    {
	    public long Value
        {
            get => InternalDecrypt();
            set
            {
	            currentCryptoKey = GenerateKey();
	            hiddenValue = Encrypt(value, currentCryptoKey);
	            
	            inited = true;
            }
        }
        
        public ELong(long value)
        {
            currentCryptoKey = GenerateKey();
            hiddenValue = Encrypt(value, currentCryptoKey);
            
            inited = true;
        }
        
        
        
        
        
        
        
        
        //Embed
        [SerializeField]
        private long currentCryptoKey;

        [SerializeField]
        private long hiddenValue;

        [SerializeField]
        private bool inited;
        
        
        
        
        
        /// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static long Encrypt(long value, long key)
		{
			return value ^ key;
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static long Decrypt(long value, long key)
		{
			return value ^ key;
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
		public static ELong FromEncrypted(long encrypted, long key)
		{
			var instance = new ELong();
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
		public long GetEncrypted(out long key)
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
		public void SetEncrypted(long encrypted, long key)
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
		public long GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private long InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(0, currentCryptoKey);
				inited = true;

				return 0;
			}

			var decrypted = Decrypt(hiddenValue, currentCryptoKey);

			return decrypted;
		}

		#region operators, overrides, interface implementations
		
		private static ELong Increment(ELong input, int increment)
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
			return obj is ELong && Equals((ELong)obj);
		}

		public bool Equals(ELong obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(ELong other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(long other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
#if !ACTK_UWP_NO_IL2CPP
			return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is long)) throw new ArgumentException("Argument must be long");
			return CompareTo((long)obj);
#endif
		}

		#endregion
		
		//! @endcond
    }
}