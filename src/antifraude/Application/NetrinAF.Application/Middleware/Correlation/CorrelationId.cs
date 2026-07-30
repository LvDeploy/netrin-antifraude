using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetrinAF.Application.Middleware.Correlation
{
    public class CorrelationId()
    {
        private static readonly AsyncLocal<string?> Current = new();

        public string Get()
        {
            return Current.Value
                ?? string.Empty;
        }

        public void Set(string? value = null)
        {
            Current.Value = string.IsNullOrWhiteSpace(value) ? Guid.NewGuid().ToString() : value;
        }
    }
}
