using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aysa.PPEMobile.Service.Helper
{
    public class QueryHelper
    {

        private readonly Dictionary<string, string> parameters;

        public QueryHelper()
        {
            this.parameters = new Dictionary<string, string>();
        }

        public QueryHelper(Dictionary<string, string> parameters)
        {
            this.parameters = parameters;
        }

        public QueryHelper(Uri uri) : this(uri.ToString())
        {
        }

        public QueryHelper(string uri)
            : this()
        {
            this.Append(uri);
        }

        public string this[string index]
        {
            get
            {
                if (this.parameters.ContainsKey(index))
                {
                    return this.parameters[index];
                }

                return null;
            }

            set
            {
                if (this.parameters.ContainsKey(index))
                {
                    this.parameters[index] = value;
                }
                else
                {
                    this.parameters.Add(index, value);
                }
            }
        }

        public void Append(string uri)
        {
            var query = (uri.IndexOf('?') > -1) ? uri.Substring(uri.IndexOf('?') + 1) : uri;
            var parts = query.Split('&');
            foreach (var data in parts.Select(s => s.Split('=')))
            {
                this.parameters.Add(data[0], data[1]);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var parameter in this.parameters)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                sb.AppendFormat("{0}={1}", parameter.Key, parameter.Value);
            }

            return sb.ToString();
        }
    }
}