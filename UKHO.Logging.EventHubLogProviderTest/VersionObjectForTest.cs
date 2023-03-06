// British Crown Copyright © 2023,
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
using System.Linq;

namespace UKHO.Logging.EventHubLogProviderTest
{
    internal class VersionObjectForTest
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }

        public VersionObjectForTest()
        {
        }

        public VersionObjectForTest(string version)
        {
            var components = version.Split('.').Select(int.Parse).ToArray();
            if (components.Length != 4)
                throw new ArgumentException($"Invalid version {version} argument", nameof(version));
            MajorVersion = components[0];
            MinorVersion = components[1];
            Build = components[2];
            Revision = components[3];
        }

        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{Build}.{Revision}";
        }
    }
}