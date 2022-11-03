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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("ChargeMessage", Schema = "Charges")]
    public class ChargeMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(35)]
        public string SenderProvidedChargeId { get; set; }

        public int Type { get; set; }

        [Required]
        [StringLength(35)]
        public string MarketParticipantId { get; set; }

        [Required]
        [StringLength(255)]
        public string MessageId { get; set; }

        [Required]
        public int MessageType { get; set; }

        [Required]
        public DateTime MessageDateTime { get; set; }
    }
}
