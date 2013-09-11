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
				this.WriteMessageToDisk(_message);
			}
		}


		public IEnumerable<Message> GetAll(
			bool purgeMessages = true)
		{
			var _messageFilePaths = Directory.GetFiles(this.c_directoryPath, "*.json");
			var _result = _messageFilePaths
				.OrderBy(messageFilePath => messageFilePath)
				.Select(filePath => this.ReadMessageFromDisk(filePath))
				.ToArray();

			if (purgeMessages) { this.DeleteMessageFiles(_messageFilePaths); }

			return _result;
		}


		private void WriteMessageToDisk(
			Message message)
		{
			var _fileName = string.Format("{0:yyyyMMddHHmmssfffffff}_{1}.{2}_{3}.json",
				DateTime.UtcNow,
				message.GetType().Namespace,
				message.GetType().Name,
				message.Id);
			var _filePath = Path.Combine(this.c_directoryPath, _fileName);

			var _messageJson = JsonConvert.SerializeObject(message);

			File.WriteAllText(_filePath, _messageJson, Encoding.Default);
		}


		private Message ReadMessageFromDisk(
			string filePath)
		{
			var _messageTypeName = this.GetMessageTypeName(filePath);
			var _messageType = Type.GetType(_messageTypeName);

			var _messageJson = File.ReadAllText(filePath, Encoding.Default);
			
			return (Message)JsonConvert.DeserializeObject(_messageJson, _messageType);
		}


		private string GetMessageTypeName(
			string filePath)
		{
			// Assumes no underscore in message type name
			return Path.GetFileName(filePath).Split('_')[1];
		}


		private void DeleteMessageFiles(
			IEnumerable<string> filePaths)
		{
			foreach(var _filePath in filePaths) { File.Delete(_filePath); }
		}
	}
}