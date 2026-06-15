import axios from "axios";

const client = axios.create({ baseURL: import.meta.env.VITE_API_URL });

client.interceptors.request.use((config) => {
  const token = localStorage.getItem("wh_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

client.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401 && localStorage.getItem("wh_token")) {
      localStorage.removeItem("wh_token");
      localStorage.removeItem("wh_player_id");
      localStorage.removeItem("wh_player_name");
      window.location.href = "/";
    }
    return Promise.reject(error);
  }
);

export default client;
