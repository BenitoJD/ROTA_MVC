using ROTA_MVC.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ROTA_MVC.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClientService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- ADD USER REGISTRATION IMPLEMENTATION ---
        public async Task<UserDetailDto?> RegisterUserAsync(RegisterUserDto registerDto)
        {
            var client = GetHttpClientWithToken(); // Registration requires Admin token
            try
            {
                // Assumes API endpoint POST /api/auth/register exists
                var response = await client.PostAsJsonAsync("api/auth/register", registerDto);

                if (response.IsSuccessStatusCode) // Expect 200 OK or maybe 201 Created (check your API)
                {
                    // API might return the created user details or just a success message.
                    // If it returns details, deserialize them. Adjust DTO if needed.
                    try
                    {
                        // Let's assume API returns OK with just a message, not the user DTO
                        // If API returned the UserDetailDto:
                        // return await response.Content.ReadFromJsonAsync<UserDetailDto?>(_jsonOptions);

                        // For now, let's return a placeholder indicating success but without full details
                        // You might need to call GetUserByUsername or similar after successful registration
                        // if you need the full DTO immediately.
                        Console.WriteLine($"User registration successful for username: {registerDto.Username}");
                        // Returning null here signifies success but no returned object from API,
                        // but this is ambiguous. Better to return bool or specific result object.
                        // Let's modify interface/impl to return bool for simplicity now.
                        return new UserDetailDto { Username = registerDto.Username }; // Return minimal DTO indicating success
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"JSON Error deserializing register response: {jsonEx.Message}");
                        // Fall through to return null or handle error
                    }
                    return null; // Or throw if deserialization fails but status was success
                }
                else
                {
                    // Read error message from API response body if possible
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error registering user: {(int)response.StatusCode} - {errorContent}");
                    // Throw exception or return null based on status code?
                    // For now, just indicate failure:
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error registering user: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }
        public async Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.GetAsync($"api/leavetypes/{id}");
                if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<LeaveTypeDto?>(_jsonOptions);
                Console.WriteLine($"API Error fetching leave type {id}: {(int)response.StatusCode}"); return null;
            }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error fetching leave type {id}: {ex.StatusCode} - {ex.Message}"); return null; }
        }

        public async Task<LeaveTypeDto?> CreateLeaveTypeAsync(CreateLeaveTypeDto leaveType)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PostAsJsonAsync("api/leavetypes", leaveType);
                if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<LeaveTypeDto?>(_jsonOptions);
                Console.WriteLine($"API Error creating leave type: {(int)response.StatusCode}"); return null;
            }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error creating leave type: {ex.StatusCode} - {ex.Message}"); return null; }
        }

        public async Task<bool> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeDto leaveType)
        {
            var client = GetHttpClientWithToken();
            try { var response = await client.PutAsJsonAsync($"api/leavetypes/{id}", leaveType); return response.IsSuccessStatusCode; }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error updating leave type {id}: {ex.StatusCode} - {ex.Message}"); return false; }
        }

        public async Task<bool> DeleteLeaveTypeAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.DeleteAsync($"api/leavetypes/{id}");
                // Check for 400 Bad Request specifically, as API prevents deleting used types
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine($"API Info: Attempted to delete leave type {id} which is likely in use.");
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error deleting leave type {id}: {ex.StatusCode} - {ex.Message}"); return false; }
        }

        // --- END LEAVE TYPE ---

        // --- ADD SHIFT TYPE CRUD IMPLEMENTATIONS ---

        public async Task<ShiftTypeDto?> GetShiftTypeByIdAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.GetAsync($"api/shifttypes/{id}");
                if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<ShiftTypeDto?>(_jsonOptions);
                Console.WriteLine($"API Error fetching shift type {id}: {(int)response.StatusCode}"); return null;
            }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error fetching shift type {id}: {ex.StatusCode} - {ex.Message}"); return null; }
        }

        public async Task<ShiftTypeDto?> CreateShiftTypeAsync(CreateShiftTypeDto shiftType)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PostAsJsonAsync("api/shifttypes", shiftType);
                if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<ShiftTypeDto?>(_jsonOptions);
                Console.WriteLine($"API Error creating shift type: {(int)response.StatusCode}"); return null;
            }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error creating shift type: {ex.StatusCode} - {ex.Message}"); return null; }
        }

        public async Task<bool> UpdateShiftTypeAsync(int id, UpdateShiftTypeDto shiftType)
        {
            var client = GetHttpClientWithToken();
            try { var response = await client.PutAsJsonAsync($"api/shifttypes/{id}", shiftType); return response.IsSuccessStatusCode; }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error updating shift type {id}: {ex.StatusCode} - {ex.Message}"); return false; }
        }

        public async Task<bool> DeleteShiftTypeAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try { var response = await client.DeleteAsync($"api/shifttypes/{id}"); return response.IsSuccessStatusCode; }
            catch (HttpRequestException ex) { Console.WriteLine($"API Error deleting shift type {id}: {ex.StatusCode} - {ex.Message}"); return false; }
        }
        // --- END SHIFT TYPE ---
        private HttpClient GetHttpClientWithToken()
        {
            var client = _httpClientFactory.CreateClient("RotaApiClient");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }


        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
        {
            var client = GetHttpClientWithToken();
            try
            {
                var employees = await client.GetFromJsonAsync<IEnumerable<EmployeeDto>>("api/employees", _jsonOptions);
                return employees ?? Enumerable.Empty<EmployeeDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching employees: {ex.StatusCode} - {ex.Message}");
                // Decide how to handle - throw custom exception, return empty list, return null?
                return Enumerable.Empty<EmployeeDto>(); // Simple handling for now
            }
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.GetAsync($"api/employees/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeDto?>(_jsonOptions);
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching employee {id}: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto employee)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PostAsJsonAsync("api/employees", employee);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeDto?>(_jsonOptions);
                }
                else
                {
                    Console.WriteLine($"API Error creating employee: {(int)response.StatusCode}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error creating employee: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto employee)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PutAsJsonAsync($"api/employees/{id}", employee);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error updating employee {id}: {ex.StatusCode} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.DeleteAsync($"api/employees/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error deleting employee {id}: {ex.StatusCode} - {ex.Message}");
                return false;
            }
        }
        public async Task<IEnumerable<ShiftDto>> GetShiftsAsync(DateTime? startDate, DateTime? endDate, int? employeeId, int? teamId, bool? isOnCall)
        {
            var client = GetHttpClientWithToken();
            // Build query string carefully
            var queryBuilder = new StringBuilder("api/shifts?");
            if (startDate.HasValue) queryBuilder.Append($"startDate={startDate.Value:yyyy-MM-dd}&");
            if (endDate.HasValue) queryBuilder.Append($"endDate={endDate.Value:yyyy-MM-dd}&");
            if (employeeId.HasValue) queryBuilder.Append($"employeeId={employeeId.Value}&");
            if (teamId.HasValue) queryBuilder.Append($"teamId={teamId.Value}&");
            if (isOnCall.HasValue) queryBuilder.Append($"isOnCall={isOnCall.Value.ToString().ToLower()}&");
            // Remove trailing '&' or '?'
            string queryString = queryBuilder.ToString().TrimEnd('&', '?');


            try
            {
                var shifts = await client.GetFromJsonAsync<IEnumerable<ShiftDto>>(queryString, _jsonOptions);
                return shifts ?? Enumerable.Empty<ShiftDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching shifts: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<ShiftDto>();
            }
        }

        public async Task<ShiftDto?> GetShiftByIdAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.GetAsync($"api/shifts/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ShiftDto?>(_jsonOptions);
                }
                return null; // Not found or other error
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching shift {id}: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<ShiftDto?> CreateShiftAsync(CreateShiftDto shift)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PostAsJsonAsync("api/shifts", shift);
                if (response.IsSuccessStatusCode) // Expect 201 Created
                {
                    return await response.Content.ReadFromJsonAsync<ShiftDto?>(_jsonOptions);
                }
                else
                {
                    Console.WriteLine($"API Error creating shift: {(int)response.StatusCode}");
                    // TODO: Optionally read and return error details from response body if API provides ProblemDetails
                    return null; // Indicate failure
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error creating shift: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }
        public async Task<IEnumerable<UserDetailDto>> GetUsersAsync()
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint GET /api/admin/users exists
                var users = await client.GetFromJsonAsync<IEnumerable<UserDetailDto>>("api/admin/users", _jsonOptions);
                return users ?? Enumerable.Empty<UserDetailDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching users: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<UserDetailDto>();
            }
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int userId)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint GET /api/admin/users/{userId} exists
                var response = await client.GetAsync($"api/admin/users/{userId}");
                if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<UserDetailDto?>(_jsonOptions);
                Console.WriteLine($"API Error fetching user {userId}: {(int)response.StatusCode}"); return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching user {userId}: {ex.StatusCode} - {ex.Message}"); return null;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, UpdateUserRoleDto roleUpdate)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint PATCH /api/admin/users/{userId}/role exists
                var response = await client.PatchAsJsonAsync($"api/admin/users/{userId}/role", roleUpdate);
                return response.IsSuccessStatusCode; // Expect 204 No Content
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error updating user role {userId}: {ex.StatusCode} - {ex.Message}"); return false;
            }
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, UpdateUserStatusDto statusUpdate)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint PATCH /api/admin/users/{userId}/status exists
                var response = await client.PatchAsJsonAsync($"api/admin/users/{userId}/status", statusUpdate);
                return response.IsSuccessStatusCode; // Expect 204 No Content
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error updating user status {userId}: {ex.StatusCode} - {ex.Message}"); return false;
            }
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint GET /api/roles exists (or similar)
                var roles = await client.GetFromJsonAsync<IEnumerable<RoleDto>>("api/roles", _jsonOptions); // Adjust URL if needed
                return roles?.OrderBy(r => r.RoleName) ?? Enumerable.Empty<RoleDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching roles: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<RoleDto>();
            }
        }
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var client = GetHttpClientWithToken(); // Gets token for the current user
            try
            {
                // Assumes API endpoint POST /api/auth/change-password exists
                var response = await client.PostAsJsonAsync("api/auth/change-password", changePasswordDto);

                if (response.IsSuccessStatusCode) // Expect 204 No Content ideally
                {
                    return (true, null); // Success
                }
                else
                {
                    // Attempt to read error message if API returns one (e.g., ProblemDetails or simple string)
                    string? errorMessage = $"API Error: {(int)response.StatusCode}";
                    try
                    {
                        // Check for ProblemDetails first
                        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
                        {
                            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(_jsonOptions);
                            errorMessage = problem?.Detail ?? problem?.Title ?? errorMessage;
                        }
                        else
                        {
                            // Otherwise just read as string
                            var errorContent = await response.Content.ReadAsStringAsync();
                            if (!string.IsNullOrWhiteSpace(errorContent)) { errorMessage = errorContent; }
                        }
                    }
                    catch { /* Ignore deserialization errors */ }

                    Console.WriteLine($"API Error changing password: {(int)response.StatusCode} - {errorMessage}");
                    return (false, errorMessage); // Failure with message
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error changing password: {ex.StatusCode} - {ex.Message}");
                return (false, "A network error occurred while contacting the API."); // Network error
            }
            catch (Exception ex) // Catch other potential errors (e.g., JSON serialization before sending)
            {
                Console.WriteLine($"Error calling ChangePassword API: {ex.Message}");
                return (false, "An unexpected error occurred.");
            }
        }
        public async Task<bool> UpdateShiftAsync(int id, UpdateShiftDto shift)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PutAsJsonAsync($"api/shifts/{id}", shift);
                return response.IsSuccessStatusCode; // Expect 204
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error updating shift {id}: {ex.StatusCode} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteShiftAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.DeleteAsync($"api/shifts/{id}");
                return response.IsSuccessStatusCode; // Expect 204
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error deleting shift {id}: {ex.StatusCode} - {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<EmployeeDto>> GetActiveEmployeesBriefAsync()
        {
            var allEmployees = await GetEmployeesAsync(); // Use existing method
            return allEmployees.Where(e => e.IsActive).OrderBy(e => e.LastName).ThenBy(e => e.FirstName);
        }

        public async Task<IEnumerable<ShiftTypeDto>> GetShiftTypesAsync()
        {
            var client = GetHttpClientWithToken();
            try
            {
                var types = await client.GetFromJsonAsync<IEnumerable<ShiftTypeDto>>("api/shifttypes", _jsonOptions);
                return types?.OrderBy(t => t.TypeName) ?? Enumerable.Empty<ShiftTypeDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching shift types: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<ShiftTypeDto>();
            }
        }
        public async Task<UserDetailDto?> GetMyProfileAsync()
        {
            var client = GetHttpClientWithToken(); // Gets token for the current user
            try
            {
                // Call the API endpoint that returns the current user's profile
                // Ensure this URL matches your API endpoint (e.g., /api/auth/me or /api/users/me)
                string requestUri = "api/auth/me";

                var response = await client.GetAsync(requestUri);

                if (response.IsSuccessStatusCode) // Expect 200 OK
                {
                    // Deserialize the response content into UserDetailDto
                    var profile = await response.Content.ReadFromJsonAsync<UserDetailDto?>(_jsonOptions);
                    return profile; // Can be null if API returns empty success or deserialization fails
                }
                else
                {
                    // Log the error if the API call was not successful
                    Console.WriteLine($"API Error fetching user profile: {(int)response.StatusCode} - {response.ReasonPhrase}");
                    // Optionally read error content: var error = await response.Content.ReadAsStringAsync();
                    return null; // Indicate failure to fetch profile
                }
            }
            catch (HttpRequestException ex) // Catch network or fundamental request errors
            {
                Console.WriteLine($"API Network Error fetching user profile: {ex.StatusCode} - {ex.Message}");
                return null; // Return null on error
            }
            catch (JsonException jsonEx) // Catch errors during JSON deserialization
            {
                Console.WriteLine($"JSON Error parsing user profile: {jsonEx.Message}");
                return null;
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                Console.WriteLine($"Unexpected error in GetMyProfileAsync: {ex.Message}");
                return null;
            }
        }
        public async Task<IEnumerable<TeamDto>> GetTeamsAsync()
        {
            var client = GetHttpClientWithToken();
            try
            {
                var teams = await client.GetFromJsonAsync<IEnumerable<TeamDto>>("api/teams", _jsonOptions);
                return teams?.OrderBy(t => t.TeamName) ?? Enumerable.Empty<TeamDto>(); 
            }
            catch (HttpRequestException ex)
            {
                // Log the error
                Console.WriteLine($"API Error fetching teams: {ex.StatusCode} - {ex.Message}");
                // Return empty list on error so dropdowns don't crash
                return Enumerable.Empty<TeamDto>();
            }
        }
        public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsAsync(
            DateTime? startDate, DateTime? endDate,
            int? employeeId, int? teamId, int? leaveTypeId, LeaveStatus? status)
        {
            var client = GetHttpClientWithToken();
            // Build query string
            var queryParams = new Dictionary<string, string?>();
            if (startDate.HasValue) queryParams.Add("startDate", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate.HasValue) queryParams.Add("endDate", endDate.Value.ToString("yyyy-MM-dd"));
            if (employeeId.HasValue) queryParams.Add("employeeId", employeeId.Value.ToString());
            if (teamId.HasValue) queryParams.Add("teamId", teamId.Value.ToString());
            if (leaveTypeId.HasValue) queryParams.Add("leaveTypeId", leaveTypeId.Value.ToString());
            if (status.HasValue) queryParams.Add("status", ((int)status.Value).ToString()); // Send enum as int? API needs to handle

            string queryString = "api/leaverequests";
            if (queryParams.Any())
            {
                queryString += "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            }


            try
            {
                var requests = await client.GetFromJsonAsync<IEnumerable<LeaveRequestDto>>(queryString, _jsonOptions);
                return requests ?? Enumerable.Empty<LeaveRequestDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching leave requests: {ex.StatusCode} - {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Access denied to view leave requests.");
                }
                return Enumerable.Empty<LeaveRequestDto>();
            }
        }

        public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.GetAsync($"api/leaverequests/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LeaveRequestDto?>(_jsonOptions);
                }
                Console.WriteLine($"API Error fetching leave request {id}: {(int)response.StatusCode}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching leave request {id}: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<LeaveRequestDto?> CreateLeaveRequestAsync(CreateLeaveRequestDto leaveRequest)
        {
            var client = GetHttpClientWithToken();
            try
            {
                var response = await client.PostAsJsonAsync("api/leaverequests", leaveRequest);
                if (response.IsSuccessStatusCode) // Expect 201 Created
                {
                    return await response.Content.ReadFromJsonAsync<LeaveRequestDto?>(_jsonOptions);
                }
                else
                {
                    Console.WriteLine($"API Error creating leave request: {(int)response.StatusCode}");
                    return null; // Indicate failure
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error creating leave request: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<LeaveRequestDto?> UpdateLeaveRequestStatusAsync(int id, UpdateLeaveStatusDto statusUpdate)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // API endpoint is PATCH /api/leaverequests/{id}/status
                var response = await client.PatchAsJsonAsync($"api/leaverequests/{id}/status", statusUpdate);
                if (response.IsSuccessStatusCode) // Expect 200 OK with updated DTO
                {
                    return await response.Content.ReadFromJsonAsync<LeaveRequestDto?>(_jsonOptions);
                }
                else
                {
                    Console.WriteLine($"API Error updating leave status {id}: {(int)response.StatusCode}");
                    // TODO: Read ProblemDetails
                    return null; // Indicate failure
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error updating leave status {id}: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CancelLeaveRequestAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // API endpoint is PATCH /api/leaverequests/{id}/cancel (doesn't need a body)
                // Send empty content for PATCH if required, or adjust API if DELETE is preferred for cancel
                HttpContent? content = null; // Can be null for PATCH if API allows
                                             // content = new StringContent("", Encoding.UTF8, "application/json"); // Or empty JSON object
                var response = await client.PatchAsync($"api/leaverequests/{id}/cancel", content);
                return response.IsSuccessStatusCode; // Expect 204 No Content
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error cancelling leave request {id}: {ex.StatusCode} - {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync()
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes your API has GET /api/leavetypes
                var types = await client.GetFromJsonAsync<IEnumerable<LeaveTypeDto>>("api/leavetypes", _jsonOptions);
                return types?.OrderBy(lt => lt.LeaveTypeName) ?? Enumerable.Empty<LeaveTypeDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching leave types: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<LeaveTypeDto>();
            }
        }
        public async Task<IEnumerable<PendingCountDto>> GetPendingLeaveCountAsync(int? teamId)
        {
            var client = GetHttpClientWithToken();
            string requestUri = "api/dashboard/leave/pendingcount";
            if (teamId.HasValue)
            {
                requestUri += $"?teamId={teamId.Value}";
            }

            try
            {
                var result = await client.GetFromJsonAsync<IEnumerable<PendingCountDto>>(requestUri, _jsonOptions);
                return result ?? Enumerable.Empty<PendingCountDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching pending leave count: {ex.StatusCode} - {ex.Message}");
                if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("Access denied to fetch pending leave count (likely non-Admin).");
                }
                return Enumerable.Empty<PendingCountDto>(); 
            }
        }

        public async Task<IEnumerable<UpcomingOnCallDto>> GetUpcomingOnCallAsync(DateTime startDate, DateTime endDate, int? teamId)
        {
            var client = GetHttpClientWithToken();
            var queryParams = new Dictionary<string, string?> {
                 { "startDate", startDate.ToString("yyyy-MM-dd") },
                 { "endDate", endDate.ToString("yyyy-MM-dd") }
             };
            if (teamId.HasValue) queryParams.Add("teamId", teamId.Value.ToString());

            string queryString = "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            string requestUri = $"api/dashboard/oncall/upcoming{queryString}";


            try
            {
                var result = await client.GetFromJsonAsync<IEnumerable<UpcomingOnCallDto>>(requestUri, _jsonOptions);
                return result ?? Enumerable.Empty<UpcomingOnCallDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching upcoming on-call: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<UpcomingOnCallDto>();
            }
        }
        public async Task<IEnumerable<LeaveSummaryDto>> GetLeaveSummaryAsync(LeaveSummaryRequestParams parameters)
        {
            var client = GetHttpClientWithToken();
            // Build query string from parameters object
            var queryParams = new Dictionary<string, string?>();
            // Add parameters if they have values
            if (parameters.StartDate.HasValue) queryParams.Add("startDate", parameters.StartDate.Value.ToString("yyyy-MM-dd"));
            if (parameters.EndDate.HasValue) queryParams.Add("endDate", parameters.EndDate.Value.ToString("yyyy-MM-dd"));
            if (parameters.EmployeeId.HasValue) queryParams.Add("employeeId", parameters.EmployeeId.Value.ToString());
            if (parameters.TeamId.HasValue) queryParams.Add("teamId", parameters.TeamId.Value.ToString());
            if (parameters.LeaveTypeId.HasValue) queryParams.Add("leaveTypeId", parameters.LeaveTypeId.Value.ToString());
            queryParams.Add("groupBy", parameters.GroupBy.ToString()); // Add the grouping parameter

            string queryString = "?" + string.Join("&", queryParams.Where(kvp => kvp.Value != null) // Filter out nulls just in case
                                                            .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            string requestUri = $"api/dashboard/leave/summary{queryString}";

            try
            {
                var result = await client.GetFromJsonAsync<IEnumerable<LeaveSummaryDto>>(requestUri, _jsonOptions);
                return result ?? Enumerable.Empty<LeaveSummaryDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching leave summary: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<LeaveSummaryDto>();
            }
        }

        public async Task<IEnumerable<ShiftTypeDistributionDto>> GetShiftTypeDistributionAsync(DateTime startDate, DateTime endDate, int? teamId)
        {
            var client = GetHttpClientWithToken();
            // Build query string
            var queryParams = new Dictionary<string, string?> {
                 { "startDate", startDate.ToString("yyyy-MM-dd") },
                 { "endDate", endDate.ToString("yyyy-MM-dd") }
             };
            if (teamId.HasValue) queryParams.Add("teamId", teamId.Value.ToString());

            string queryString = "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            string requestUri = $"api/dashboard/shifts/typedistribution{queryString}";

            try
            {
                var result = await client.GetFromJsonAsync<IEnumerable<ShiftTypeDistributionDto>>(requestUri, _jsonOptions);
                return result ?? Enumerable.Empty<ShiftTypeDistributionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching shift type distribution: {ex.StatusCode} - {ex.Message}");
                return Enumerable.Empty<ShiftTypeDistributionDto>();
            }
        }
        public async Task<TeamDto?> GetTeamByIdAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint GET /api/teams/{id} exists
                var response = await client.GetAsync($"api/teams/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TeamDto?>(_jsonOptions);
                }
                Console.WriteLine($"API Error fetching team {id}: {(int)response.StatusCode}");
                return null; // Not found or other error
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error fetching team {id}: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<TeamDto?> CreateTeamAsync(CreateTeamDto team)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint POST /api/teams exists
                var response = await client.PostAsJsonAsync("api/teams", team);
                if (response.IsSuccessStatusCode) // Expect 201 Created
                {
                    return await response.Content.ReadFromJsonAsync<TeamDto?>(_jsonOptions);
                }
                else
                {
                    Console.WriteLine($"API Error creating team: {(int)response.StatusCode}");
                    // TODO: Read ProblemDetails if API returns them on 400 (e.g., name exists)
                    return null; // Indicate failure
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error creating team: {ex.StatusCode} - {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateTeamAsync(int id, UpdateTeamDto team)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint PUT /api/teams/{id} exists
                var response = await client.PutAsJsonAsync($"api/teams/{id}", team);
                return response.IsSuccessStatusCode; // Expect 204
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error updating team {id}: {ex.StatusCode} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            var client = GetHttpClientWithToken();
            try
            {
                // Assumes API endpoint DELETE /api/teams/{id} exists
                var response = await client.DeleteAsync($"api/teams/{id}");
                return response.IsSuccessStatusCode; // Expect 204
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error deleting team {id}: {ex.StatusCode} - {ex.Message}");
                // Check for 400 Bad Request specifically if API prevents deletion when team is in use
                // if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest) { /* Handle differently? */ }
                return false;
            }
        }
    }
}
