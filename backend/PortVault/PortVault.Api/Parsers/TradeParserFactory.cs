namespace PortVault.Api.Parsers
{
    public class TradeParserFactory
    {
        private readonly IEnumerable<ITradeParser> _parsers;
        public TradeParserFactory(IEnumerable<ITradeParser> parsers)
        {
            _parsers = parsers;
        }

        public ITradeParser Get(string provider)
        {
            return _parsers.First(x => x.Provider == provider);
        }
    }

}
