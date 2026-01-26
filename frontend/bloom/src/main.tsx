import { createRoot } from 'react-dom/client'
import './assets/reset.css'
import './assets/screen.css'
import App from './App.tsx'
import { BrowserRouter } from "react-router-dom";

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>,
)
