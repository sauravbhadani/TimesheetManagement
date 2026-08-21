import { useCallback, useEffect, useState } from "react";
import { describeApiError } from "../../api/client";
import { projectsApi } from "../../api/projectsApi";
import type { ProjectDto, ProjectTaskDto, TaskClassification } from "../../api/types";

const CLASSIFICATIONS: TaskClassification[] = ["CapEx", "OpEx"];

export function ProjectsPage() {
  const [projects, setProjects] = useState<ProjectDto[]>([]);
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null);
  const [tasks, setTasks] = useState<ProjectTaskDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [newName, setNewName] = useState("");
  const [newCode, setNewCode] = useState("");
  const [newDescription, setNewDescription] = useState("");

  const [newTaskName, setNewTaskName] = useState("");
  const [newTaskClassification, setNewTaskClassification] = useState<TaskClassification>("OpEx");
  const [newTaskBillable, setNewTaskBillable] = useState(false);

  const loadProjects = useCallback(async () => {
    setLoading(true);
    try {
      const data = await projectsApi.getAll(true);
      setProjects(data);
      if (!selectedProjectId && data.length > 0) setSelectedProjectId(data[0].id);
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    void loadProjects();
  }, [loadProjects]);

  useEffect(() => {
    if (!selectedProjectId) {
      setTasks([]);
      return;
    }
    projectsApi
      .getTasks(selectedProjectId, true)
      .then(setTasks)
      .catch((err) => setError(describeApiError(err)));
  }, [selectedProjectId]);

  async function handleCreateProject(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const created = await projectsApi.create({ name: newName, code: newCode, description: newDescription || null });
      setNewName("");
      setNewCode("");
      setNewDescription("");
      await loadProjects();
      setSelectedProjectId(created.id);
    } catch (err) {
      setError(describeApiError(err));
    }
  }

  async function handleToggleProjectActive(project: ProjectDto) {
    setError(null);
    try {
      await projectsApi.update(project.id, {
        name: project.name,
        code: project.code,
        description: project.description,
        isActive: !project.isActive,
      });
      await loadProjects();
    } catch (err) {
      setError(describeApiError(err));
    }
  }

  async function handleCreateTask(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedProjectId) return;
    setError(null);
    try {
      await projectsApi.createTask({
        projectId: selectedProjectId,
        name: newTaskName,
        description: null,
        classification: newTaskClassification,
        isBillable: newTaskBillable,
      });
      setNewTaskName("");
      setNewTaskClassification("OpEx");
      setNewTaskBillable(false);
      setTasks(await projectsApi.getTasks(selectedProjectId, true));
    } catch (err) {
      setError(describeApiError(err));
    }
  }

  async function handleToggleTaskActive(task: ProjectTaskDto) {
    setError(null);
    try {
      await projectsApi.updateTask(task.id, {
        name: task.name,
        description: task.description,
        classification: task.classification,
        isBillable: task.isBillable,
        isActive: !task.isActive,
      });
      if (selectedProjectId) setTasks(await projectsApi.getTasks(selectedProjectId, true));
    } catch (err) {
      setError(describeApiError(err));
    }
  }

  const selectedProject = projects.find((p) => p.id === selectedProjectId) ?? null;

  return (
    <div>
      <h1 className="h4 mb-3">Projects</h1>
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="row g-4">
        <div className="col-lg-6">
          <div className="card mb-3">
            <div className="card-body">
              <h2 className="h6">New Project</h2>
              <form onSubmit={handleCreateProject} className="row g-2">
                <div className="col-md-6">
                  <input className="form-control" placeholder="Name" value={newName} onChange={(e) => setNewName(e.target.value)} required />
                </div>
                <div className="col-md-3">
                  <input className="form-control" placeholder="Code" value={newCode} onChange={(e) => setNewCode(e.target.value)} required />
                </div>
                <div className="col-md-3">
                  <button type="submit" className="btn btn-primary w-100">
                    Add
                  </button>
                </div>
                <div className="col-12">
                  <input
                    className="form-control"
                    placeholder="Description (optional)"
                    value={newDescription}
                    onChange={(e) => setNewDescription(e.target.value)}
                  />
                </div>
              </form>
            </div>
          </div>

          {loading ? (
            <div className="text-muted">Loading…</div>
          ) : (
            <table className="table table-hover align-middle">
              <thead className="table-light">
                <tr>
                  <th>Name</th>
                  <th>Code</th>
                  <th>Active</th>
                </tr>
              </thead>
              <tbody>
                {projects.map((p) => (
                  <tr
                    key={p.id}
                    className={p.id === selectedProjectId ? "table-active" : ""}
                    style={{ cursor: "pointer" }}
                    onClick={() => setSelectedProjectId(p.id)}
                  >
                    <td>{p.name}</td>
                    <td>{p.code}</td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="form-check form-switch">
                        <input
                          className="form-check-input"
                          type="checkbox"
                          checked={p.isActive}
                          onChange={() => void handleToggleProjectActive(p)}
                        />
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="col-lg-6">
          {selectedProject ? (
            <>
              <h2 className="h6">Tasks — {selectedProject.name}</h2>
              <div className="card mb-3">
                <div className="card-body">
                  <form onSubmit={handleCreateTask} className="row g-2 align-items-end">
                    <div className="col-md-5">
                      <label className="form-label small text-muted">Name</label>
                      <input className="form-control" value={newTaskName} onChange={(e) => setNewTaskName(e.target.value)} required />
                    </div>
                    <div className="col-md-3">
                      <label className="form-label small text-muted">Classification</label>
                      <select
                        className="form-select"
                        value={newTaskClassification}
                        onChange={(e) => setNewTaskClassification(e.target.value as TaskClassification)}
                      >
                        {CLASSIFICATIONS.map((c) => (
                          <option key={c} value={c}>
                            {c}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="col-md-2">
                      <div className="form-check">
                        <input
                          className="form-check-input"
                          type="checkbox"
                          id="new-task-billable"
                          checked={newTaskBillable}
                          onChange={(e) => setNewTaskBillable(e.target.checked)}
                        />
                        <label className="form-check-label small" htmlFor="new-task-billable">
                          Billable
                        </label>
                      </div>
                    </div>
                    <div className="col-md-2">
                      <button type="submit" className="btn btn-primary w-100">
                        Add
                      </button>
                    </div>
                  </form>
                </div>
              </div>

              <table className="table table-hover align-middle">
                <thead className="table-light">
                  <tr>
                    <th>Name</th>
                    <th>Class.</th>
                    <th>Billable</th>
                    <th>Active</th>
                  </tr>
                </thead>
                <tbody>
                  {tasks.length === 0 && (
                    <tr>
                      <td colSpan={4} className="text-center text-muted py-3">
                        No tasks yet.
                      </td>
                    </tr>
                  )}
                  {tasks.map((t) => (
                    <tr key={t.id}>
                      <td>{t.name}</td>
                      <td>{t.classification}</td>
                      <td>{t.isBillable ? "Yes" : "No"}</td>
                      <td>
                        <div className="form-check form-switch">
                          <input
                            className="form-check-input"
                            type="checkbox"
                            checked={t.isActive}
                            onChange={() => void handleToggleTaskActive(t)}
                          />
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          ) : (
            <div className="text-muted">Select a project to manage its tasks.</div>
          )}
        </div>
      </div>
    </div>
  );
}
