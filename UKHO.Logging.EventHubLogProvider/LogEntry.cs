﻿// British Crown Copyright © 2018,
// All rights reserved.
// 
// You may not copy the Software, rent, lease, sub-license, loan, translate, merge, adapt, vary
// re-compile or modify the Software without written permission from UKHO.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL CROWN OR THE SECRETARY OF STATE FOR DEFENCE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
// IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
// OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace UKHO.Logging.EventHubLogProvider
{
    public interface ILogEntry
    {
        DateTime Timestamp { get; set; }
        string Level { get; set; }
        string MessageTemplate { get; set; }
        Dictionary<string, object> LogProperties { get; set; }
        EventId EventId { get; set; }
        Exception Exception { get; set; }
    }

    public class LogEntry : ILogEntry
    {
        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("Level")]
        public string Level { get; set; }

        [JsonProperty("MessageTemplate")]
        public string MessageTemplate { get; set; }

        [JsonProperty("Properties")]
        public Dictionary<string, object> LogProperties { get; set; }

        public EventId EventId { get; set; }
        public Exception Exception { get; set; }
    }
}