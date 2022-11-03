// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    public static class CimXmlHelper
    {
        /// <summary>
        /// Utility method for only adding an element if it is needed
        /// </summary>
        /// <param name="cimNamespace">The namespace of the element to possibly add</param>
        /// <param name="elementName">The name of the element to possibly add</param>
        /// <param name="getValue">Method used to retrieve the value of the element. Note: Lazy, so only invoked if needed</param>
        /// <param name="isNeeded">Whether the element is needed or not. Default is 'true'</param>
        /// <returns>Empty list if the element is not needed or a list with a single element if the element was needed.
        /// This will allow us to make sure an element either is skipped or not</returns>
        public static IEnumerable<XElement> GetElementIfNeeded(
            XNamespace cimNamespace,
            string elementName,
            Func<object> getValue,
            bool isNeeded = true)
        {
            return isNeeded
                ? new List<XElement> { new(cimNamespace + elementName, getValue.Invoke()) }
                : new List<XElement>();
        }
    }
}
