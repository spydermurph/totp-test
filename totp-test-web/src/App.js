import {BrowserRouter, Routes, Route} from "react-router-dom";
import Login from "./login";
import Twofactor from "./twofactor";
import LoginOTP from "./loginotp";
import TwofactorOTP from "./twofactorotp";
import Genre from "./genre";
import LoginTOTP from "./logintotp";
import RegisterTOTP from "./registertotp";
import SubmitTOTP from "./submittotp";
import LoginIdenTOTP from "./loginidentotp";
import RegisterIdenTOTP from "./registeridentotp";
import SubmitIdenTOTP from "./submitidentotp";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route index element={<Genre />}></Route>
        <Route path="/login" element={<Login />}></Route>
        <Route path="/twofactor" element={<Twofactor />}></Route>
        <Route path="/loginotp" element={<LoginOTP />}></Route>
        <Route path="/twofactorotp" element={<TwofactorOTP />}></Route>
        <Route path="/logintotp" element={<LoginTOTP />}></Route>
        <Route path="/registertotp" element={<RegisterTOTP />}></Route>
        <Route path="/submittotp" element={<SubmitTOTP />}></Route>
        <Route path="/loginidentotp" element={<LoginIdenTOTP />}></Route>
        <Route path="/registeridentotp" element={<RegisterIdenTOTP />}></Route>
        <Route path="/submitidentotp" element={<SubmitIdenTOTP />}></Route>
        <Route path="*" element={<Genre />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
