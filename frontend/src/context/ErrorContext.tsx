import { createContext, useContext, useEffect, useState } from "react";
import { setErrorHandler } from "../api/client";

type ErrorContextType = {
  error: string | null;
  clearError: () => void;
};

const ErrorContext = createContext<ErrorContextType>({
  error: null,
  clearError: () => {},
});

export const ErrorProvider = ({ children }: { children: React.ReactNode }) => {
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setErrorHandler(setError);
    return () => setErrorHandler(() => {});
  }, []);

  return (
    <ErrorContext.Provider value={{ error, clearError: () => setError(null) }}>
      {children}
    </ErrorContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useError = () => useContext(ErrorContext);
