using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace PMCG.Messaging.Client
{
	// This will not scale !
	// Use
	//		http://redis.io/
	//		http://managedesent.codeplex.com/	- ravendb storage engine
	public class FileSystemDisconnectedMessageStore : IDisconnectedMessageStore
	{
		private readonly string c_directoryPath;


		public static readonly string FileExtension = ".message";


		public FileSystemDisconnectedMessageStore(
			string directoryPath)
		{
			this.c_directoryPath = directoryPath;
		}


		public IEnumerable<Guid> GetAllIds()
		{
			return Directory.GetFiles(this.c_directoryPath, "*" + FileSystemDisconnectedMessageStore.FileExtension)
				.OrderBy(filePath => filePath)
				.Select(filePath => this.GetMessageIdFromFilePath(filePath))
				.ToArray();
		}


		public void Add(
			Message message)
		{
			var _nowAsString = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fffffff");
			var _messageType = message.GetType();
			var _messageJson = JsonConvert.SerializeObject(message);

			var _fileName = string.Format("{0}_{1}.{2}_{3}{4}",
				_nowAsString,
				_messageType.Namespace,
				_messageType.Name,
				message.Id,
				FileSystemDisconnectedMessageStore.FileExtension);
			var _filePath = Path.Combine(this.c_directoryPath, _fileName);

			var _fileContent = string.Format("{1}{0}{2}{0}{3}{0}{0}{4}",
				Environment.NewLine,
				_nowAsString,
				_messageType.AssemblyQualifiedName,							// Version issues, seem okay
				message.Id,
				_messageJson);

			File.WriteAllText(_filePath, _fileContent, Encoding.Default);
		}


		public Message Get(
			Guid id)
		{
			var _fileSearchPattern = string.Format("*_{0}{1}", id, FileSystemDisconnectedMessageStore.FileExtension);
			var _filePath = Directory.GetFiles(this.c_directoryPath, _fileSearchPattern).First();

			var _fileContentAsLines = File.ReadAllLines(_filePath, Encoding.Default);
			var _messageTypeAssemblyQualifiedName = _fileContentAsLines[1];
			var _messageJson = string.Join(Environment.NewLine, _fileContentAsLines.Skip(4));

			var _messageType = Type.GetType(_messageTypeAssemblyQualifiedName);

			return (Message)JsonConvert.DeserializeObject(_messageJson, _messageType);
		}


		public void Delete(
			Guid id)
		{
			File.Delete(this.GetFilePathForMessageId(id));
		}


		private Guid GetMessageIdFromFilePath(
			string key)
		{
			var _keyPart = key.Substring(key.LastIndexOf('_') + 1).Replace(FileSystemDisconnectedMessageStore.FileExtension, string.Empty);
			return new Guid(_keyPart);
		}


		private string GetFilePathForMessageId(
			Guid id)
		{
			var _fileSearchPattern = string.Format("*_{0}{1}", id, FileSystemDisconnectedMessageStore.FileExtension);
			return Directory.GetFiles(this.c_directoryPath, _fileSearchPattern).First();
		}
	}
}