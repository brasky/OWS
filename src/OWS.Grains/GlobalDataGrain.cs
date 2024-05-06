using OWS.Interfaces;
using OWSData.Models.Composites;
using OWSData.Models.Tables;
using OWSData.Repositories.Interfaces;
using OWSGlobalData.DTOs;

namespace OWS.Grains
{
    public sealed class GlobalDataGrain : BaseGrain, IGlobalDataGrain
    {
        private readonly IGlobalDataRepository _globalDataRepository;

        public GlobalDataGrain(IGlobalDataRepository globalDataRepository)
        {
            _globalDataRepository = globalDataRepository;
        }

        public async Task<SuccessAndErrorMessage> AddOrUpdateGlobalDataItem(AddOrUpdateGlobalDataItemDTO request)
        {
            var globalDataToAdd = new GlobalData()
            {
                CustomerGuid = GetCustomerId(),
                GlobalDataKey = request.GlobalDataKey,
                GlobalDataValue = request.GlobalDataValue
            };

            await _globalDataRepository.AddOrUpdateGlobalData(globalDataToAdd);

            var successAndErrorMessage = new SuccessAndErrorMessage()
            {
                Success = true,
                ErrorMessage = ""
            };

            return successAndErrorMessage;
        }
    }
}
