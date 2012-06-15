using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.IO;

namespace DoubanFM
{
	/// <summary>
	/// 提供文件打包功能
	/// </summary>
	public static class FilePackage
	{
		/// <summary>
		/// 复制流中的内容
		/// </summary>
		/// <param name="source">源流</param>
		/// <param name="target">目标流</param>
		private static void CopyStream(Stream source, Stream target)
		{
			const int bufSize = 0x1000;
			byte[] buf = new byte[bufSize];
			int bytesRead = 0;
			while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
				target.Write(buf, 0, bytesRead);
		}

		/// <summary>
		/// 创建一个包
		/// </summary>
		/// <param name="packagePath">新创建的包的保存位置</param>
		/// <param name="baseDirectory">指定要打包的文件的根目录，留空表示工作目录</param>
		/// <param name="filePaths">要打包的文件的路径</param>
		public static void CreatePackage(string packagePath, string baseDirectory, params string[] filePaths)
		{
			using (Package package = ZipPackage.Open(packagePath, System.IO.FileMode.Create))
			{
				foreach (var filePath in filePaths)
				{
					PackagePart part = package.CreatePart(PackUriHelper.CreatePartUri(new Uri(filePath, UriKind.RelativeOrAbsolute)), "application/octet-stream");
					using (FileStream fileStream = new FileStream(baseDirectory == null ? filePath : Path.Combine(baseDirectory, filePath), FileMode.Open, FileAccess.Read))
					{
						CopyStream(fileStream, part.GetStream());
					}
				}
			}
		}

		/// <summary>
		/// 提取一个包中的内容
		/// </summary>
		/// <param name="packagePath">要提取的包的路径</param>
		/// <param name="targetDirectory">指定提取出的文件的存放文件夹，留空表示工作目录</param>
		public static void ExtractPackage(string packagePath, string targetDirectory)
		{
			using (Package package = ZipPackage.Open(packagePath, FileMode.Open, FileAccess.Read))
			{
				foreach (var part in package.GetParts())
				{
					string targetPath = targetDirectory == null ? part.Uri.ToString().TrimStart('/') : Path.Combine(targetDirectory, part.Uri.ToString().TrimStart('/'));
					Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
					using (FileStream fileStream = new FileStream(targetPath, FileMode.Create))
					{
						CopyStream(part.GetStream(), fileStream);
					}
				}
			}
		}
	}
}
