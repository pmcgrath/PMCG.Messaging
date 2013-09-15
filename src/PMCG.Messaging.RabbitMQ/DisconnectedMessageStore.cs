using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace PMCG.Messaging.RabbitMQ
{
	public class DisconnectedMessageStore
	{
		private readonly string c_directoryPath;


		public static readonly string FileExtension = ".message";


		public DisconnectedMessageStore(
			string directoryPath)
		{
			this.c_directoryPath = directoryPath;
		}


		public void Store(
			params Message[] messages)
		{
			foreach (var _message in messages)
			{
				this.WriteMessage(_message);
			}
		}


		public IEnumerable<string> GetAllMessageKeys()
		{
			var _messageFilePaths = Directory.GetFiles(this.c_directoryPath, "*" + DisconnectedMessageStore.FileExtension);
			return _messageFilePaths.OrderBy(filePath => filePath).ToArray();
		}


		public string WriteMessage(
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
				DisconnectedMessageStore.FileExtension);
			var _filePath = Path.Combine(this.c_directoryPath, _fileName);

			var _fileContent = string.Format("{1}{0}{2}{0}{3}{0}{0}{4}",
				Environment.NewLine,
				_nowAsString,
				_messageType.AssemblyQualifiedName,							// Version issues, seem okay
				message.Id,
				_messageJson);

			File.WriteAllText(_filePath, _fileContent, Encoding.Default);

			return _filePath;
		}


		public Message ReadMessage(
			string key)
		{
			var _fileContentAsLines = File.ReadAllLines(key, Encoding.Default);
			var _messageTypeAssemblyQualifiedName = _fileContentAsLines[1];
			var _messageJson = string.Join(Environment.NewLine, _fileContentAsLines.Skip(4));

			var _messageType = Type.GetType(_messageTypeAssemblyQualifiedName);

			return (Message)JsonConvert.DeserializeObject(_messageJson, _messageType);
		}


		public void RemoveMessage(
			string key)
		{
			File.Delete(key);
		}


		public Guid GetMessageIdFromKey(
			string key)
		{
			var _keyPart = key.Substring(key.LastIndexOf('_') + 1).Replace(DisconnectedMessageStore.FileExtension, string.Empty);
			return new Guid(_keyPart);
		}
	}
}