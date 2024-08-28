import { useState } from "react";
import { useNavigate } from "react-router-dom";

const LoginTOTP = () => {
  const [emailAddress, setEmailAddress] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    const blog = { emailAddress, password };

    try{
      const response = await fetch('https://localhost:44363/api/accounts/logintotp', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(blog)
      });
      console.log(response);
      if(response.status === 200) {
        let data = await response.json();
        console.log(data);
        if(data.isTwoFactor === true && data.tokenUri !== null){
          //navigate('Twofactor', {state:{email:data.emailAddress}});
          navigate('/registertotp', {state: {email:data.email, provider:data.provider, token:data.token, tokenuri:data.tokenuri}});
        }
        if(data.isRegistered) {
          console.log("Go to submit");
          navigate('/submittotp', {state: {email:data.email, provider:data.provider}});
        }
      } else if( response.status === 401) {
        alert("Invalid username or password");
      } else {
        alert("Unknown error");
      }
    } catch(err) {
      console.log(err);
      alert("Could not connect to server");
    }
    /*.then((response) => {
        return response.json();
    }).then((data) => {
        console.log(data);
        if(data.isTwoFactor === true && data.tokenUri !== null){
            //navigate('Twofactor', {state:{email:data.emailAddress}});
          navigate('/registertotp', {state: {email:data.email, provider:data.provider, token:data.token, tokenuri:data.tokenuri}});
        }
        if(data.isRegistered) {
          console.log("Go to submit");
          navigate('/submittotp', {state: {email:data.email, provider:data.provider}});
        }
    }).catch((error) => {
      console.log(error);
      alert("Could not connect to server");
    })*/
  }

  return (
    <div className="logintotp">
      <h2>Login TOTP</h2>
      <form onSubmit={handleSubmit}>
        <label>Name:</label>
        <input 
          type="text" 
          required 
          value={emailAddress}
          onChange={(e) => setEmailAddress(e.target.value)}
        />
        <label>Password:</label>
        <input
          type="text"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        ></input>
        <button>Login</button>
      </form>
    </div>
  );
}
 
export default LoginTOTP;