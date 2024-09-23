using System;
using UnityEngine;
using VPackage.SaveGameSystem.Security.EmbedAntiCheat;

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !ENABLE_IL2CPP
#define ACTK_UWP_NO_IL2CPP
#endif


namespace VPackage.SaveGameSystem.Security
{
    [Serializable]
    public struct EString : IObscuredType
    {
	    private bool isNull;
        public string Value
        {
	        get => isNull ? null : InternalDecryptToString();
	        set
	        {
		        isNull = value == null;
		        
		        currentCryptoKey = null;
		        hiddenValue = null;
		        
		        cryptoKey = new char[7];
		        GenerateKey(ref cryptoKey);
		        
		        if(value != null)
					hiddenChars = InternalEncryptDecrypt(value.ToCharArray(), cryptoKey);

		        inited = true;
	        }
        }

        public EString(string value)
        {
	        isNull = value == null;
	        
	        currentCryptoKey = null;
	        hiddenValue = null;
	        
	        cryptoKey = new char[7];
            GenerateKey(ref cryptoKey);
            
            if(value != null)
	            hiddenChars = InternalEncryptDecrypt(value.ToCharArray(), cryptoKey);
            else
	            hiddenChars = null;


            inited = true;
        }
        
        
        
        
        
        
        
        //Embed
        [SerializeField]
        private string currentCryptoKey; // deprecated

#pragma warning disable 0649
        [SerializeField]
        private byte[] hiddenValue; // deprecated
#pragma warning restore 0649

        [SerializeField]
        private char[] cryptoKey;

        [SerializeField]
        private char[] hiddenChars;

        [SerializeField]
        public bool inited;

        
        
        
        
        /// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static char[] Encrypt(string value, string key)
		{
			return Encrypt(value, key.ToCharArray());
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static char[] Encrypt(string value, char[] key)
		{
			return Encrypt(value.ToCharArray(), key);
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static char[] Encrypt(char[] value, char[] key)
		{
			return InternalEncryptDecrypt(value, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static string Decrypt(char[] value, string key)
		{
			return Decrypt(value, key.ToCharArray());
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static string Decrypt(char[] value, char[] key)
		{
			return new string(InternalEncryptDecrypt(value, key));
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
		public static EString FromEncrypted(char[] encrypted, char[] key)
		{
			var instance = new EString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Use this only to decrypt data encrypted with previous ACTk versions.
		/// </summary>
		/// Please use \ref FromEncrypted() "FromEncrypted(char[], char[])" in other cases.
		[Obsolete("Use this only to decrypt data encrypted with previous ACTk versions. " +
		          "Please use FromEncrypted(char[], char[]) in other cases.")]
		public static EString FromEncrypted(string encrypted, string key = "4441")
		{
			var instance = new EString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key in new allocated array. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static char[] GenerateKey()
		{
			var arrayToFill = new char[7];
			GenerateKey(ref arrayToFill);
			return arrayToFill;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <param name="arrayToFill">Preallocated char array. Only first 7 bytes are filled.</param>
		public static void GenerateKey(ref char[] arrayToFill)
		{
			RandomUtils.GenerateCharArrayKey(ref arrayToFill);
		}

		[Obsolete("Please use version with ref argument or without arguments instead.")]
		public static char[] GenerateKey(char[] arrayToFill)
		{
			RandomUtils.GenerateCharArrayKey(ref arrayToFill);
			return arrayToFill;
		}

		internal static char[] InternalEncryptDecrypt(char[] value, char[] key)
		{
			if (value == null || value.Length == 0)
			{
				return value;
			}

			if (key.Length == 0)
			{
				Debug.LogError(ACTkConstants.LogPrefix + "Empty key can't be used for string encryption or decryption!");
				return value;
			}

			var keyLength = key.Length;
			var valueLength = value.Length;

			var result = new char[valueLength];

			for (var i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return result;
		}

		internal static string EncryptDecryptObsolete(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError(ACTkConstants.LogPrefix + "Empty key can't be used for string encryption or decryption!");
				return string.Empty;
			}

			var keyLength = key.Length;
			var valueLength = value.Length;

			var result = new char[valueLength];

			for (var i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return new string(result);
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// \sa FromEncrypted(), SetEncrypted()
		public char[] GetEncrypted(out char[] key)
		{
			key = cryptoKey;
			return hiddenChars;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// \sa FromEncrypted()
		public void SetEncrypted(char[] encrypted, char[] key)
		{
			inited = true;
			hiddenChars = encrypted;
			cryptoKey = key;
		}

		/// <summary>
		/// Use this only to decrypt data encrypted with previous ACTk versions.
		/// </summary>
		/// Please use \ref SetEncrypted() "SetEncrypted(char[], char[])" in other cases.
		[Obsolete("Use this only to decrypt data encrypted with previous ACTk versions. " +
		          "Please use SetEncrypted(char[], char[]) in other cases.")]
		public void SetEncrypted(string encrypted, string key)
		{
			inited = true;
			var decrypted = EncryptDecryptObsolete(encrypted, key);
			cryptoKey = GenerateKey();
			hiddenChars = Encrypt(decrypted, cryptoKey);
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public string GetDecrypted()
		{
			return InternalDecryptToString();
		}

		/// <summary>
		/// GC-friendly alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value as a raw chars array in case you don't wish to allocate new string.</returns>
		public char[] GetDecryptedToChars()
		{
			return InternalDecrypt();
		}

		/// <summary>
		/// Allows to change current crypto key to the new random value and re-encrypt variable using it.
		/// Use it for extra protection against 'unknown value' search.
		/// Just call it sometimes when your variable doesn't change to fool the cheater.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly produces some GC allocations, be careful when using it!</strong>
		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			GenerateKey(ref cryptoKey);
			hiddenChars = InternalEncryptDecrypt(decrypted, cryptoKey); // encrypting
		}

		private string InternalDecryptToString()
		{
			return new string(InternalDecrypt());
		}

		private char[] InternalDecrypt()
		{
			if (!inited)
			{
				cryptoKey = new char[7];
				GenerateKey(ref cryptoKey);
				hiddenChars = InternalEncryptDecrypt(new char[0], cryptoKey); // encrypting
				inited = true;

				return new char[0];
			}

			if (!string.IsNullOrEmpty(currentCryptoKey))
			{
				MigrateFromACTkV1();
			}

			var decrypted = InternalEncryptDecrypt(hiddenChars, cryptoKey);

			return decrypted;
		}

		private bool CompareCharsToString(char[] chars, string s)
		{
			if (chars.Length != s.Length) return false;

			for (var i = 0; i < chars.Length; i++)
			{
				if (chars[i] != s[i])
				{
					return false;
				}
			}

			return true;
		}

		internal void MigrateFromACTkV1()
		{
			var decryptedOld = EncryptDecryptObsolete(GetStringObsolete(hiddenValue), currentCryptoKey);
			GenerateKey(ref cryptoKey);
			hiddenChars = InternalEncryptDecrypt(decryptedOld.ToCharArray(), cryptoKey);
			currentCryptoKey = null;
		}

		#region operators, overrides, interface implementations

		//! @cond
		public int Length
		{
			get { return hiddenChars.Length; }
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public char this[int index]
		{
			get
			{
				if (index < 0 || index >= Length)
				{
					throw new IndexOutOfRangeException();
				}

				return InternalDecrypt()[index];
			}
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public string Substring(int startIndex)
		{
			return Substring(startIndex, Length - startIndex);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public string Substring(int startIndex, int length)
		{
			return InternalDecryptToString().Substring(startIndex, length);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public bool StartsWith(string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return InternalDecryptToString().StartsWith(value, comparisonType);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public bool EndsWith(string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return InternalDecryptToString().EndsWith(value, comparisonType);
		}

		public override int GetHashCode()
		{
			return InternalDecryptToString().GetHashCode();
		}

		public override string ToString()
		{
			return new string(InternalDecrypt());
		}

		#endregion

		//! @endcond

		internal static string GetStringObsolete(byte[] bytes)
		{
			var chars = new char[bytes.Length / sizeof(char)];
			Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		internal static byte[] GetBytesObsolete(string str)
		{
			var bytes = new byte[str.Length * sizeof(char)];
			Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private static bool ArraysEquals(char[] a1, char[] a2)
		{
			if (a1 == a2) return true;
			if (a1 == null || a2 == null) return false;
			if (a1.Length != a2.Length) return false;

			for (var i = 0; i < a1.Length; i++)
			{
				if (a1[i] != a2[i])
				{
					return false;
				}
			}
			return true;
		}
    }
}