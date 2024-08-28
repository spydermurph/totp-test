import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import QRCode from "react-qr-code";

const RegisterIdenTOTP = () => {
  const [token, setToken] = useState('');
  const navigate = useNavigate();
  const location = useLocation();
  const email = location.state.email;
  const provider = location.state.provider;
  const totpToken = location.state.token;
  const tokenUri = location.state.tokenuri;

  const handleSubmit = async (e) => {
    e.preventDefault();
    const blog = { token, email, provider };

    try {
      const response = await fetch('https://localhost:44363/api/accounts/registeridentitytotp', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(blog)
      });
      if(response.status === 200) {
        let data = await response.json();
        if(data.token !== null) {
          localStorage.setItem("token", data.token);
          navigate('/');
        } else {
          console.log("JWT missing");
        }
      } else if(response.status === 401) {
        console.log("Incorrect code");
        alert("Incorrect code")
      } else {
        console.log("Unknown error");
        alert("Unknown error");
      }
    } catch (err) {
      console.log(err);
      alert("Could not connect to server");
    }
    /*.then((response) => {
      return response.json();
    }).then((data) => {
      if(data.token !== null) {
        localStorage.setItem("token", data.token);
        navigate('/');
      }
    })*/
  }

  return (
    <div className="registeridentotp">
      <center>
        <h2>Register Identity Totp</h2>
        <div style={{ background: 'white', padding: '16px' }}>
          {/*tokenUri === null ? "" : <QRCode value={tokenUri} />*/}
          <QRCode value={"otpauth://totp/Testissuer:arnarhaf@test.is?secret=" + totpToken + "&issuer=Testissuer"} />
        </div>
        <p>{totpToken}</p>
        <form onSubmit={handleSubmit}>
          <label>Token:</label>
          <input 
            type="text" 
            required 
            value={token}
            onChange={(e) => setToken(e.target.value)}
          />
          <button>Submit</button>
        </form>
        {/*<p>{email}</p>
        <p>{provider}</p>
        <p>{totpToken}</p>
        <p>{tokenUri}</p>*/}
      </center>
    </div>

  );
}
 
export default RegisterIdenTOTP;