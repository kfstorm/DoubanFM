using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Bass
{
	public class BassInitializationFailureException : Exception
	{
		public Un4seen.Bass.BASSError Code { get; private set; }

		public static string GetErrorMessage(Un4seen.Bass.BASSError code)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("音频组件初始化失败： ");
			string detail = null;
			switch (code)
			{
				case Un4seen.Bass.BASSError.BASS_ERROR_DX:
					detail = "未安装DirectX。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_DEVICE:
					detail = "无效的设备。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_ALREADY:
					detail = "设备已经初始化。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_DRIVER:
					detail = "没有可用的设备驱动，设备可能正在使用。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_FORMAT:
					detail = "设备不支持此格式。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_MEM:
					detail = "内存不足。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_NO3D:
					detail = "无法初始化3D支持。";
					break;
				case Un4seen.Bass.BASSError.BASS_ERROR_UNKNOWN:
				default:
					detail = "未知错误。";
					break;
			}
			sb.Append(detail);
			return sb.ToString();
		}

		public BassInitializationFailureException(Un4seen.Bass.BASSError code)
			: base(GetErrorMessage(code))
		{
			Code = code;
		}
	}
}
