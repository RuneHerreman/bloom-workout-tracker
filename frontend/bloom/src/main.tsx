import { createRoot } from 'react-dom/client'
import './assets/css/reset.css'
import './assets/css/screen.css'
import App from './App.tsx'
import { BrowserRouter } from "react-router-dom";

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>,
)
