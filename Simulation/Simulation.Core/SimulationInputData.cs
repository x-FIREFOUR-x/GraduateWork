using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Simulation.InputData.Game;
using Simulation.InputData.Map;
using Simulation.InputData.Serialization;


namespace Simulation
{
    [Serializable]
    public class SimulationInputData
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new PublicFieldsContractResolver(),
            Converters = { new StringEnumConverter() },
            MissingMemberHandling = MissingMemberHandling.Error,
            Formatting = Formatting.Indented
        };

        public GameData Data = new GameData();
        public MapData Map = new MapData();

        public static SimulationInputData Load(string path) => Parse(File.ReadAllText(path));

        public void Save(string path) => File.WriteAllText(path, ToJson());

        public static SimulationInputData Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Simulation input is empty.", nameof(json));

            SimulationInputData input = JsonConvert.DeserializeObject<SimulationInputData>(json, SerializerSettings);
            if (input?.Data == null || input.Map == null)
                throw new ArgumentException("Simulation input has no game data or map.", nameof(json));

            return input;
        }

        public string ToJson() => JsonConvert.SerializeObject(this, SerializerSettings);
    }
}
