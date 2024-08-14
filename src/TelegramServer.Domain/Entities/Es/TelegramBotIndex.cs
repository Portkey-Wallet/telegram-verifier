using System;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace TelegramServer.Entities.Es;

public class TelegramBotIndex : TelegramEsEntity<Guid>, IIndexBuild
{
    [Keyword] public string BotId { get; set; }
    [Keyword] public string PlaintextSecret { get; set; }
    [Keyword] public string Secret { get; set; }
    
    [Keyword] public long CreateTime { get; set; }
}