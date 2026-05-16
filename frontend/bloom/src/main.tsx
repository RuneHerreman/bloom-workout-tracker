import { createRoot } from 'react-dom/client'
import './assets/css/reset.css'
import './assets/css/screen.css'
import App from './App.tsx'
import { createBrowserRouter, RouterProvider } from "react-router-dom";

const router = createBrowserRouter([{ path: "*", element: <App /> }]);

createRoot(document.getElementById('root')!).render(
  <RouterProvider router={router} />
)
