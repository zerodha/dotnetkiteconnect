using System;
using System.Collections.Generic;

namespace KiteConnect
{
    internal readonly struct ParametersBuilder
    {
        private readonly List<KeyValuePair<string, string>> _parameters;
        public ParametersBuilder()
        {
            _parameters = new List<KeyValuePair<string, string>>();
        }

        public ParametersBuilder Add(string key, string value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public ParametersBuilder Add(string key, bool value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value.ToString()));
            return this;
        }

        public ParametersBuilder AddAsO1(string key, bool value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value ? "1" : "0"));
            return this;
        }

        public ParametersBuilder Add(string key, DateTime value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value.ToString("yyyy-MM-dd HH:mm:ss")));
            return this;
        }

        public ParametersBuilder AddIfNotNull(string key, string value)
        {
            if (value != null)
                _parameters.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public ParametersBuilder Add(string key, int value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value.ToString()));
            return this;
        }

        public ParametersBuilder Add(string key, decimal value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value.ToString()));
            return this;
        }

        public ParametersBuilder AddIfNotNull(string key, int? value)
        {
            if (value != null)
                _parameters.Add(new KeyValuePair<string, string>(key, value.ToString()));
            return this;
        }

        public ParametersBuilder AddIfNotNull(string key, decimal? value)
        {
            if (value != null)
                _parameters.Add(new KeyValuePair<string, string>(key, value.ToString()));
            return this;
        }

        public IReadOnlyCollection<KeyValuePair<string, string>> Build()
        {
            return _parameters;
        }
    }
}
