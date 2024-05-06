﻿using Orleans;

namespace OWSData.Models.Composites
{
    [GenerateSerializer]
    public class CustomCharacterDataDTO
    {
        public string CustomFieldName { get; set; }
        public string FieldValue { get; set; }
    }
}
