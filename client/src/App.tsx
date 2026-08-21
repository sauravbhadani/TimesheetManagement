import { Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./auth/AuthContext";
import { LoginPage } from "./auth/LoginPage";
import { RequireRole } from "./auth/RequireRole";
import { BrandingProvider } from "./branding/BrandingContext";
import { AppLayout } from "./layout/AppLayout";
import { AllTimesheetsPage } from "./pages/admin/AllTimesheetsPage";
import { BrandingConfigPage } from "./pages/admin/BrandingConfigPage";
import { ProjectsPage } from "./pages/admin/ProjectsPage";
import { UsersPage } from "./pages/admin/UsersPage";
import { MyHistoryPage } from "./pages/employee/MyHistoryPage";
import { MyTimesheetPage } from "./pages/employee/MyTimesheetPage";
import { ReviewTimesheetPage } from "./pages/manager/ReviewTimesheetPage";
import { TeamTimesheetsPage } from "./pages/manager/TeamTimesheetsPage";

function HomeRedirect() {
  const { status } = useAuth();
  if (status === "loading") return null;
  return <Navigate to={status === "signed-in" ? "/my-timesheet" : "/login"} replace />;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<AppLayout />}>
        <Route path="/" element={<HomeRedirect />} />

        <Route
          path="/my-timesheet"
          element={
            <RequireRole roles={["Employee", "Manager", "Admin"]}>
              <MyTimesheetPage />
            </RequireRole>
          }
        />
        <Route
          path="/my-history"
          element={
            <RequireRole roles={["Employee", "Manager", "Admin"]}>
              <MyHistoryPage />
            </RequireRole>
          }
        />

        <Route
          path="/team"
          element={
            <RequireRole roles={["Manager", "Admin"]}>
              <TeamTimesheetsPage />
            </RequireRole>
          }
        />
        <Route
          path="/team/:id"
          element={
            <RequireRole roles={["Manager", "Admin"]}>
              <ReviewTimesheetPage />
            </RequireRole>
          }
        />

        <Route
          path="/admin/projects"
          element={
            <RequireRole roles={["Admin"]}>
              <ProjectsPage />
            </RequireRole>
          }
        />
        <Route
          path="/admin/users"
          element={
            <RequireRole roles={["Admin"]}>
              <UsersPage />
            </RequireRole>
          }
        />
        <Route
          path="/admin/all-timesheets"
          element={
            <RequireRole roles={["Admin"]}>
              <AllTimesheetsPage />
            </RequireRole>
          }
        />
        <Route
          path="/admin/branding"
          element={
            <RequireRole roles={["Admin"]}>
              <BrandingConfigPage />
            </RequireRole>
          }
        />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export function App() {
  return (
    <BrandingProvider>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrandingProvider>
  );
}
