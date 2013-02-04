using System;
using System.Web;
using System.Reflection;

//use a more obscure namespace to prevent type name collisions.
namespace Ensoft.Web.Configuration
{
	/// <summary>
	/// Provides a hook into the current ASP.Net MachineKey parameters.
	/// </summary>
	public class MachineKeyWrapper
	{
		private static MethodInfo _encOrDecData;
		private static MethodInfo _hexStringToByteArray;
		private static MethodInfo _byteArrayToHexString;

		static MachineKeyWrapper()
		{
			object config = HttpContext.Current.GetConfig("system.web/machineKey");
			Type configType = config.GetType();

			Type machineKeyType = configType.Assembly.GetType("System.Web.Configuration.MachineKey");
			if (machineKeyType == null)
			{
				// try to get asp.net 2.0 type
				machineKeyType = configType.Assembly.GetType("System.Web.Configuration.MachineKeySection");
			}

			BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Static;

			_encOrDecData = machineKeyType.GetMethod("EncryptOrDecryptData", bf);
			_hexStringToByteArray = machineKeyType.GetMethod("HexStringToByteArray", bf);
			_byteArrayToHexString = machineKeyType.GetMethod("ByteArrayToHexString", bf);

			//is there any way to get some kind of pointer?  or just trust:
			// MethodBase.Invoke
			// RuntimeMethodInfo.Invoke
			// RuntimeMethodHandle.InvokeFast
			// RuntimeMethodHandle._InvokeFast
			// ...lot of extra calls...

			if( _encOrDecData==null || _hexStringToByteArray==null || _byteArrayToHexString==null )
			{
				throw new InvalidOperationException("Unable to get the methods to invoke.");
			}
		}

		// for the record, I feel that this MachineKey class should be public.  Why not have us
		// think about security only a little bit, and say, "Here... here's some nice helper
		// methods for you to encrypt data only in a tamper-proof way" 
		// anyways, whatever.

		/// <summary>
		/// Converts a hex string into a byte array.
		/// </summary>
		/// <param name="str">string to convert</param>
		/// <returns>byte array</returns>
		public static byte[] HexStringToByteArray(string str)
		{
			return (byte[]) _hexStringToByteArray.Invoke(null, new object[] { str });
		}
		/// <summary>
		/// Converts a byte array into a hex string
		/// </summary>
		/// <param name="array">array to convert</param>
		/// <param name="length">length of array to convert</param>
		/// <returns>hex string representing the byte array.</returns>
		public static string ByteArrayToHexString(byte[] array, int length)
		{
			return (string) _byteArrayToHexString.Invoke(null, new object[] { array, length });
		}
		/// <summary>
		/// Encrypts and decrypts data using ASP.Net MachineKey configuration settings.
		/// </summary>
		/// <param name="encrypting">true if encrypting, false if decrypting</param>
		/// <param name="data">the data to operate on</param>
		/// <param name="mod"></param>
		/// <param name="index">beginning index</param>
		/// <param name="length">length of array to operate on</param>
		/// <returns>encrypted or decrypted byte array</returns>
		public static byte[] EncryptOrDecryptData(bool encrypting, byte[] data, byte[] mod, int index, int length)
		{
			return (byte[])_encOrDecData.Invoke(null, new object[] { encrypting, data, mod, index, length });
			//could do a catch TargetInvocationException and make it disappear...
		}

	}
}