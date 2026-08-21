import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useBranding } from "../branding/BrandingContext";

export function AppLayout() {
  const { user, logout } = useAuth();
  const { branding } = useBranding();

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `nav-link${isActive ? " active fw-semibold" : ""}`;

  return (
    <div className="d-flex flex-column min-vh-100">
      <nav className="navbar navbar-expand-lg navbar-dark bg-primary">
        <div className="container-fluid">
          <span className="navbar-brand d-flex align-items-center gap-2">
            {branding.logoDataUrl && (
              <img src={branding.logoDataUrl} alt="" style={{ height: 28, width: 28, objectFit: "contain" }} />
            )}
            {branding.companyName}
          </span>

          {user && (
            <div className="collapse navbar-collapse">
              <ul className="navbar-nav me-auto">
                <li className="nav-item">
                  <NavLink to="/my-timesheet" className={navLinkClass}>
                    My Timesheet
                  </NavLink>
                </li>
                <li className="nav-item">
                  <NavLink to="/my-history" className={navLinkClass}>
                    My History
                  </NavLink>
                </li>

                {(user.role === "Manager" || user.role === "Admin") && (
                  <li className="nav-item">
                    <NavLink to="/team" className={navLinkClass}>
                      Team Timesheets
                    </NavLink>
                  </li>
                )}

                {user.role === "Admin" && (
                  <>
                    <li className="nav-item">
                      <NavLink to="/admin/projects" className={navLinkClass}>
                        Projects
                      </NavLink>
                    </li>
                    <li className="nav-item">
                      <NavLink to="/admin/users" className={navLinkClass}>
                        Users
                      </NavLink>
                    </li>
                    <li className="nav-item">
                      <NavLink to="/admin/all-timesheets" className={navLinkClass}>
                        All Timesheets
                      </NavLink>
                    </li>
                    <li className="nav-item">
                      <NavLink to="/admin/branding" className={navLinkClass}>
                        Branding
                      </NavLink>
                    </li>
                  </>
                )}
              </ul>

              <div className="d-flex align-items-center gap-3">
                <span className="text-white-50 small">
                  {user.fullName} <span className="badge text-bg-light text-dark ms-1">{user.role}</span>
                </span>
                <button type="button" className="btn btn-sm btn-outline-light" onClick={() => void logout()}>
                  Sign out
                </button>
              </div>
            </div>
          )}
        </div>
      </nav>

      <main className="flex-grow-1 container-fluid py-4">
        <Outlet />
      </main>
    </div>
  );
}
