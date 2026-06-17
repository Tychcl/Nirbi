using System.Collections.Generic;
using System.Text;
using nearby.Classes;
using nearby.Interfaces;
using nearby.Models;
using Newtonsoft.Json;

namespace nearby.Services
{
    public class ConfirmationService : IConfirmationService
    {
        private readonly ApiClient _apiClient;
        private readonly IUserService _userService;

        public ConfirmationService(ApiClient apiClient, IUserService us)
        {
            _apiClient = apiClient;
            _userService = us;
        }

        public async Task<Guid> CreateConfirmationAsync(CreateConfirmationRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("Confirmations", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<Guid>(json);
        }

        public async Task<Confirmation> GetConfirmationAsync(Guid id)
        {
            var response = await _apiClient.GetAsync($"Confirmations/{id}");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<Confirmation>(json)!;
        }

        public async Task<List<Confirmation>> GetConfirmationsByReviewerAsync()
        {
            var response = await _apiClient.GetAsync($"Confirmations/reviewer");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            var r = JsonConvert.DeserializeObject<List<Confirmation>>(json) ?? new List<Confirmation>();
            foreach (var conf in r)
            {
                if (!string.IsNullOrWhiteSpace(conf.MetaData))
                {
                    try
                    {
                        var meta = JsonConvert.DeserializeObject<Dictionary<string, string>>(conf.MetaData);
                        conf.TaskName = meta?.ContainsKey("taskName") == true ? meta["taskName"] : null;
                    }
                    catch { conf.TaskName = null; }
                }
            }
            return r;
        }

        public async Task<List<Confirmation>> GetConfirmationsByInitiatorAsync()
        {
            var response = await _apiClient.GetAsync($"Confirmations/initiator");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            var r = JsonConvert.DeserializeObject<List<Confirmation>>(json) ?? new List<Confirmation>();
            foreach (var conf in r)
            {
                if (!string.IsNullOrWhiteSpace(conf.MetaData))
                {
                    try
                    {
                        var meta = JsonConvert.DeserializeObject<Dictionary<string, string>>(conf.MetaData);
                        conf.TaskName = meta?.ContainsKey("taskName") == true ? meta["taskName"] : null;
                    }
                    catch { conf.TaskName = null; }
                }
            }
            return r;
        }

        public async Task<List<Confirmation>> GetConfirmationsByEntityAsync(Guid entityId)
        {
            var response = await _apiClient.GetAsync($"Confirmations/entity/{entityId}");
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);
            return JsonConvert.DeserializeObject<List<Confirmation>>(json) ?? new List<Confirmation>();
        }

        public async Task RespondToConfirmationAsync(Guid confirmationId, RespondToConfirmationRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync($"Confirmations/{confirmationId}/respond", content);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task RevokeConfirmationAsync(Guid id, Guid initiatorId)
        {
            var response = await _apiClient.PostAsync($"Confirmations/{id}/revoke?initiatorId={initiatorId}", null);
            if (response == null) throw new HttpRequestException("Ошибка подключения к серверу");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }

        public async Task<string?> GetTaskConfirmationStatus(Guid taskId)
        {
            var all = await GetConfirmationsByInitiatorAsync();
            var confirmation = all.FirstOrDefault(x => x.EntityId == taskId && x.Status != "Revoked");
            if (confirmation is null)
            {
                return null;
            }
            return confirmation.Status;
        }

        public async Task<List<Candidate>> GetTaskCandidates(Guid taskId)
        {
            var all = await GetConfirmationsByReviewerAsync();
            var confirmations = all.Where(x => x.EntityId == taskId && (x.Status == "Created" || x.Status == "Accepted" || x.Status == "Rejected")).ToList();
            var ids = confirmations.Select(x => x.InitiatorId).ToList();
            List<FullNames> names = await _userService.GetUserFullNamesAsync(ids);
            List<Candidate> candidates = new List<Candidate>();
            for (int i = 0; i < names.Count(); i++)
            {
                candidates.Add(new Candidate(names[i], confirmations[i].Id, confirmations[i].Status));
            }
            return candidates;
        }
    }
}