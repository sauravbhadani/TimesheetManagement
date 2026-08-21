import { useState } from "react";
import { useBranding, type BrandingConfig } from "../../branding/BrandingContext";

export function BrandingConfigPage() {
  const { branding, updateBranding, resetBranding } = useBranding();
  const [draft, setDraft] = useState<BrandingConfig>(branding);
  const [saved, setSaved] = useState(false);

  function handleLogoUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => setDraft((d) => ({ ...d, logoDataUrl: reader.result as string }));
    reader.readAsDataURL(file);
  }

  function handleSave(e: React.FormEvent) {
    e.preventDefault();
    updateBranding(draft);
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  }

  function handleReset() {
    resetBranding();
    setDraft(branding);
  }

  return (
    <div>
      <h1 className="h4 mb-1">Branding Config</h1>
      <p className="text-muted small">
        Applies immediately across the app and is stored locally in this browser — no server or database involved.
        Point a new deployment at its own branding by setting these values once.
      </p>

      {saved && <div className="alert alert-success py-2">Branding saved.</div>}

      <div className="row g-4 mt-1">
        <div className="col-lg-6">
          <form onSubmit={handleSave} className="card">
            <div className="card-body d-flex flex-column gap-3">
              <div>
                <label className="form-label small text-muted">Company Name</label>
                <input
                  className="form-control"
                  value={draft.companyName}
                  onChange={(e) => setDraft((d) => ({ ...d, companyName: e.target.value }))}
                  required
                />
              </div>

              <div>
                <label className="form-label small text-muted">Logo</label>
                <input type="file" accept="image/*" className="form-control" onChange={handleLogoUpload} />
                {draft.logoDataUrl && (
                  <div className="mt-2">
                    <img src={draft.logoDataUrl} alt="Logo preview" style={{ height: 40 }} />
                    <button
                      type="button"
                      className="btn btn-sm btn-link text-danger"
                      onClick={() => setDraft((d) => ({ ...d, logoDataUrl: null }))}
                    >
                      Remove
                    </button>
                  </div>
                )}
              </div>

              <div className="row g-3">
                <div className="col-6">
                  <label className="form-label small text-muted">Primary Color</label>
                  <input
                    type="color"
                    className="form-control form-control-color w-100"
                    value={draft.primaryColor}
                    onChange={(e) => setDraft((d) => ({ ...d, primaryColor: e.target.value }))}
                  />
                </div>
                <div className="col-6">
                  <label className="form-label small text-muted">Secondary Color</label>
                  <input
                    type="color"
                    className="form-control form-control-color w-100"
                    value={draft.secondaryColor}
                    onChange={(e) => setDraft((d) => ({ ...d, secondaryColor: e.target.value }))}
                  />
                </div>
              </div>

              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary">
                  Save
                </button>
                <button type="button" className="btn btn-outline-secondary" onClick={handleReset}>
                  Reset to Default
                </button>
              </div>
            </div>
          </form>
        </div>

        <div className="col-lg-6">
          <div className="small text-muted mb-2">Live Preview</div>
          <div className="border rounded overflow-hidden">
            <div className="d-flex align-items-center gap-2 px-3 py-2" style={{ background: draft.primaryColor }}>
              {draft.logoDataUrl && <img src={draft.logoDataUrl} alt="" style={{ height: 24 }} />}
              <span className="text-white fw-semibold">{draft.companyName || "Company Name"}</span>
            </div>
            <div className="p-3 d-flex gap-2">
              <button type="button" className="btn" style={{ background: draft.primaryColor, color: "#fff" }}>
                Primary Button
              </button>
              <button type="button" className="btn" style={{ background: draft.secondaryColor, color: "#fff" }}>
                Secondary Button
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
