using ROTA_MVC.Models;

namespace ROTA_MVC.Services
{
    public interface IApiClientService
    {
        Task<IEnumerable<EmployeeDto>> GetEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto employee);
        Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto employee);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

        Task<IEnumerable<ShiftDto>> GetShiftsAsync(DateTime? startDate, DateTime? endDate, int? employeeId, int? teamId, bool? isOnCall);
        Task<ShiftDto?> GetShiftByIdAsync(int id);
        Task<ShiftDto?> CreateShiftAsync(CreateShiftDto shift); // Return DTO on success
        Task<bool> UpdateShiftAsync(int id, UpdateShiftDto shift);
        Task<bool> DeleteShiftAsync(int id);


        Task<IEnumerable<EmployeeDto>> GetActiveEmployeesBriefAsync(); 

        Task<IEnumerable<TeamDto>> GetTeamsAsync();
        Task<IEnumerable<ShiftTypeDto>> GetShiftTypesAsync(); // For dropdowns

        Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsAsync(
           DateTime? startDate, DateTime? endDate,
           int? employeeId, int? teamId, int? leaveTypeId, LeaveStatus? status); // Added status
        Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id);
        Task<LeaveRequestDto?> CreateLeaveRequestAsync(CreateLeaveRequestDto leaveRequest); // Return DTO on success
        Task<LeaveRequestDto?> UpdateLeaveRequestStatusAsync(int id, UpdateLeaveStatusDto statusUpdate); // Admin only, return updated DTO
        Task<bool> CancelLeaveRequestAsync(int id); // User or Admin
        Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync(); // For dropdowns

        Task<IEnumerable<PendingCountDto>> GetPendingLeaveCountAsync(int? teamId);
        Task<IEnumerable<UpcomingOnCallDto>> GetUpcomingOnCallAsync(DateTime startDate, DateTime endDate, int? teamId);
        Task<IEnumerable<LeaveSummaryDto>> GetLeaveSummaryAsync(LeaveSummaryRequestParams parameters);
        Task<IEnumerable<ShiftTypeDistributionDto>> GetShiftTypeDistributionAsync(DateTime startDate, DateTime endDate, int? teamId);

        Task<TeamDto?> GetTeamByIdAsync(int id);    // Add this
        Task<TeamDto?> CreateTeamAsync(CreateTeamDto team); // Add this
        Task<bool> UpdateTeamAsync(int id, UpdateTeamDto team); // Add this
        Task<bool> DeleteTeamAsync(int id); // Add this

        // --- Add Leave Type CRUD Methods ---
        Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(int id);
        Task<LeaveTypeDto?> CreateLeaveTypeAsync(CreateLeaveTypeDto leaveType);
        Task<bool> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeDto leaveType);
        Task<bool> DeleteLeaveTypeAsync(int id);
        // --- End Leave Type ---

        // --- Add Shift Type CRUD Methods ---
        Task<ShiftTypeDto?> GetShiftTypeByIdAsync(int id);
        Task<ShiftTypeDto?> CreateShiftTypeAsync(CreateShiftTypeDto shiftType);
        Task<bool> UpdateShiftTypeAsync(int id, UpdateShiftTypeDto shiftType);
        Task<bool> DeleteShiftTypeAsync(int id);

       

        Task<IEnumerable<UserDetailDto>> GetUsersAsync();
        Task<UserDetailDto?> GetUserByIdAsync(int userId);
        // Creation is often handled via AuthController/Register, but could add here if needed:
        // Task<UserDetailDto?> RegisterUserAsync(RegisterUserDto registerDto);
        Task<bool> UpdateUserRoleAsync(int userId, UpdateUserRoleDto roleUpdate);
        Task<bool> UpdateUserStatusAsync(int userId, UpdateUserStatusDto statusUpdate);
        Task<IEnumerable<RoleDto>> GetRolesAsync(); // Need this for role dropdown
        Task<UserDetailDto?> RegisterUserAsync(RegisterUserDto registerDto);
        Task<UserDetailDto?> GetMyProfileAsync();
    }
}
