import { useError } from "../context/ErrorContext";

const ErrorBanner = () => {
  const { error, clearError } = useError();
  if (!error) return null;

  return (
    <div style={{
      position: "fixed",
      top: 0,
      left: 0,
      right: 0,
      background: "#c0392b",
      color: "#fff",
      padding: "12px 16px",
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      zIndex: 1000,
    }}>
      <span>{error}</span>
      <button onClick={clearError} style={{ background: "none", border: "none", color: "#fff", cursor: "pointer", fontSize: "1.2rem" }}>✕</button>
    </div>
  );
};

export default ErrorBanner;
